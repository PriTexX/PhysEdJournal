using Api.Rest.Common;
using Api.Rest.System.Contracts;
using Core.Commands;
using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Rest;

public static class SystemController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        router
            .MapGet("me", GetMe)
            .RequireAuthorization()
            .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)));
    }

    private static async Task<IResult> GetMe(
        [FromBody] MeRequest req,
        HttpContext httpCtx,
        [FromServices] ApplicationContext appCtx
    )
    {
        var guid = httpCtx.User.FindFirst(c => c.Type == "IndividualGuid")?.Value;

        if (guid is null)
        {
            return Results.Unauthorized();
        }

        if (req.Type == UserType.Student)
        {
            var studentActivity = await appCtx
                .Students.Where(s => s.StudentGuid == guid)
                .Select(s => new
                {
                    s.AdditionalPoints,
                    s.PointsForStandards,
                    s.Visits,
                    s.Group!.VisitValue,
                })
                .FirstOrDefaultAsync();

            if (studentActivity is null)
            {
                return Response.Error(new StudentNotFoundError());
            }

            var studentPoints =
                studentActivity.AdditionalPoints
                + studentActivity.PointsForStandards
                + (studentActivity.Visits * studentActivity.VisitValue);

            return Results.Ok(new StudentInfoResponse { Points = studentPoints });
        }

        var teacherPermissions = await appCtx
            .Teachers.Where(t => t.TeacherGuid == guid)
            .Select(t => new { t.Permissions })
            .FirstOrDefaultAsync();

        if (teacherPermissions is null)
        {
            return Response.Error(new TeacherNotFoundError());
        }

        var textTeacherPermissions = teacherPermissions
            .Permissions.ToString()
            .Split(",")
            .Select(s => s.Trim())
            .ToList();

        return Results.Ok(new ProfessorInfoResponse { Permisions = textTeacherPermissions });
    }
}
