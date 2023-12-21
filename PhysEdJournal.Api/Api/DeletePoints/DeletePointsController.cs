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

        router.MapPost("/DeleteStudentVisit", DeleteStudentVisit);
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
}
