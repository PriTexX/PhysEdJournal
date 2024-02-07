using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.Student;

public static class StudentController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        router.MapPost("/ActivateStudent", ActivateStudent);
        router.MapPost("/DeActivateStudent", DeActivateStudent);
    }

    public static async Task<IResult> ActivateStudent(
        string studentGuid,
        [FromServices] ActivateStudentCommand activateStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await activateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }

    public static async Task<IResult> DeActivateStudent(
        string studentGuid,
        [FromServices] DeActivateStudentCommand deActivateStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await deActivateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }
}
