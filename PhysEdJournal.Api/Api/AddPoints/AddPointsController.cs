using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.AddPoints.Contracts;
using PhysEdJournal.Api.Api.Group.Contracts;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands;
using static PhysEdJournal.Core.Constants.PermissionConstants;
using ILogger = Serilog.ILogger;

namespace PhysEdJournal.Api.Api.AddPoints;

public static class AddPointsController
{
    public static void MapAddPointsEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(GroupErrors.Errors);

        router.MapPost("/AddPointsToStudent", AddPointsToStudent);
    }

    public static async Task<IResult> AddPointsToStudent(
        [FromBody] AddPointsToStudentRequest request,
        [FromServices] AddPointsCommand addPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        [FromServices] ILogger logger,
        HttpContext ctx
    )
    {
        var callerGuid = GetCallerGuid(ctx.User);

        switch (request.WorkType)
        {
            case WorkType.InternalTeam
            or WorkType.Activist
            or WorkType.Competition:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS
                );
                break;
            }

            case WorkType.OnlineWork:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_LMS_PERMISSIONS
                );
                break;
            }
        }

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsSuccess;

        var addPointsPayload = new AddPointsCommandPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.PointsAmount,
            Date = request.Date,
            WorkType = request.WorkType,
            IsAdmin = isAdminOrSecretary,
            Comment = request.Comment,
        };

        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(
            _ => Results.Ok(),
            err =>
            {
                logger.LogWarning(err, "Something bad happened");
                return ErrorHandler.HandleErrorResult(err);
            }
        );
    }

    private static string GetCallerGuid(ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.Claims.First(c => c.Type == "IndividualGuid").Value;
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }

        return callerGuid;
    }
}
