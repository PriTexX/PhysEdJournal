using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.Semester.Contracts;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.Semester;

public static class SemesterController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(SemesterErrors.Errors);

        router.MapPost("/StartNewSemester", StartNewSemester);
    }

    public static async Task<IResult> StartNewSemester(
        string semesterName,
        [FromServices] StartNewSemesterCommand startNewSemesterCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await startNewSemesterCommand.ExecuteAsync(semesterName);

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }
}
