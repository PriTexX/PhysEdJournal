using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.Archive.Contracts;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.Archive;

public static class ArchiveController
{
    public static void MapArchiveEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(ArchiveErrors.Errors);

        router.MapPost("/ArchiveStudent", ArchiveStudent);
        router.MapPost("/UnArchiveStudent", UnArchiveStudent);
    }

    public static async Task<IResult> ArchiveStudent(
        [FromBody] ArchiveStudentRequest request,
        [FromServices] ArchiveStudentCommand archiveStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var archiveStudentPayload = new ArchiveStudentCommandPayload
        {
            StudentGuid = request.StudentGuid,
            SemesterName = request.SemesterName,
        };

        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }

    public static async Task<IResult> UnArchiveStudent(
        string studentGuid,
        string semesterName,
        [FromServices] UnArchiveStudentCommand unArchiveStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var unArchiveStudentPayload = new UnArchiveStudentCommandPayload
        {
            StudentGuid = studentGuid,
            SemesterName = semesterName,
        };

        var res = await unArchiveStudentCommand.ExecuteAsync(unArchiveStudentPayload);

        return res.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }
}
