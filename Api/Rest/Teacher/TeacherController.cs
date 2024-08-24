using Api.Rest.Common;
using Api.Rest.Teacher.Contracts;
using Core.Commands;
using DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Rest;

public static class TeacherController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        var teacherRouter = router.MapGroup("/teacher");

        teacherRouter.MapGet("/{guid}", GetTeacher);
    }

    private static async Task<IResult> GetTeacher(
        string guid,
        [FromServices] ApplicationContext appCtx
    )
    {
        var teacher = await appCtx
            .Teachers.Include(t => t.Groups)
            .FirstOrDefaultAsync(t => t.TeacherGuid == guid);

        if (teacher is null)
        {
            return Response.Error(new TeacherNotFoundError());
        }

        var permissions = teacher.Permissions.ToString().Split(",").Select(s => s.Trim()).ToList();
        var groups = teacher.Groups?.Select(g => g.GroupName).ToList() ?? [];

        return Response.Ok(
            new GetTeacherResponse
            {
                FullName = teacher.FullName,
                Groups = groups,
                Permissions = permissions,
            }
        );
    }
}
