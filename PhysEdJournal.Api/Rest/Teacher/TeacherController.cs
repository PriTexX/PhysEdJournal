using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Teacher.Contracts;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Rest.Teacher;

public static class TeacherController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(TeacherErrors.Errors);

        router.MapPost("/CreateTeacher", CreateTeacherAsync);
        router.MapPost("/GivePermissionsToTeacher", GivePermissionsToTeacherAsync);
    }

    public static async Task<IResult> CreateTeacherAsync(
        [FromBody] CreateTeacherRequest request,
        [FromServices] CreateTeacherCommand createTeacherCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var createTeacherPayload = new CreateTeacherCommandPayload
        {
            TeacherGuid = request.TeacherGuid,
            FullName = request.FullName,
            Permissions = TeacherPermissions.DefaultAccess,
        };

        var result = await createTeacherCommand.ExecuteAsync(createTeacherPayload);

        return result.Match(Response.Ok, Response.Error);
    }

    public static async Task<IResult> GivePermissionsToTeacherAsync(
        [FromBody] GivePermissionsToTeacherRequest request,
        [FromServices] GivePermissionsCommand givePermissionsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var teacherPermissions = request.Permissions.Aggregate((prev, next) => prev | next);

        if (teacherPermissions.HasFlag(TeacherPermissions.AdminAccess))
        {
            await permissionValidator.ValidateTeacherPermissionsAndThrow(
                callerGuid,
                FOR_ONLY_SUPERUSER_USE_PERMISSIONS
            );
        }
        else
        {
            await permissionValidator.ValidateTeacherPermissionsAndThrow(
                callerGuid,
                FOR_ONLY_ADMIN_USE_PERMISSIONS
            );
        }

        var givePermissionsPayload = new GivePermissionsCommandPayload
        {
            TeacherGuid = request.TeacherGuid,
            TeacherPermissions = teacherPermissions,
        };

        var result = await givePermissionsCommand.ExecuteAsync(givePermissionsPayload);

        return result.Match(Response.Ok, Response.Error);
    }
}
