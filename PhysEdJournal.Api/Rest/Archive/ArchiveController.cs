using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Archive.Contracts;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;
using Response = PhysEdJournal.Api.Rest.Common.Response;

namespace PhysEdJournal.Api.Rest.Archive;

public static class ArchiveController
{
    public static void MapArchiveEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(ArchiveErrors.Errors);

        router
            .MapPost("/ArchiveStudent", ArchiveStudent)
            .AddEndpointFilter<
                ValidationFilter<ArchiveStudentRequest, ArchiveStudentRequest.Validator>
            >();
        router
            .MapPost("/UnArchiveStudent", UnArchiveStudent)
            .AddEndpointFilter<
                ValidationFilter<UnArchiveStudentRequest, UnArchiveStudentRequest.Validator>
            >();
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

        return res.Match(Response.Ok, Response.Error);
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

        return res.Match(Response.Ok, Response.Error);
    }
}
