using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.DeletePoints.Contracts;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.DeletePoints;

public static class DeletePointsController
{
    public static void MapDeletePointsEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(DeletePointsErrors.Errors);

        router.MapDelete("/DeleteStudentVisit", DeleteStudentVisit);
        router.MapDelete("/DeletePoints", DeletePoints);
        router.MapDelete("/DeleteStandardPoints", DeleteStandardPoints);
    }

    public static async Task<IResult> DeleteStudentVisit(
        int historyId,
        [FromServices] DeleteStudentVisitCommand deleteStudentVisitCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsSuccess;

        var res = await deleteStudentVisitCommand.ExecuteAsync(
            new DeleteStudentVisitCommandPayload
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }

    public static async Task<IResult> DeletePoints(
        int historyId,
        [FromServices] DeletePointsCommand deletePointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsSuccess;

        var res = await deletePointsCommand.ExecuteAsync(
            new DeletePointsCommandPayload
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }

    public static async Task<IResult> DeleteStandardPoints(
        int historyId,
        [FromServices] DeleteStandardPointsCommand deleteStandardPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsSuccess;

        var res = await deleteStandardPointsCommand.ExecuteAsync(
            new DeleteStandardPointsCommandPayload
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }
}
