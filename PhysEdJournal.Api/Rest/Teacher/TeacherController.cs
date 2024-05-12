using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Common.Filters;
using PhysEdJournal.Api.Rest.Teacher.Contracts;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;

namespace PhysEdJournal.Api.Rest.Teacher;

public static class TeacherController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(TeacherErrors.Errors);

        var teacherRouter = router.MapGroup("/teacher");

        teacherRouter
            .MapPost("/", CreateTeacherAsync)
            .AddValidation(CreateTeacherRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);

        teacherRouter
            .MapPost("/permissions", GivePermissionsToTeacherAsync)
            .AddValidation(GivePermissionsToTeacherRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);
    }

    private static async Task<IResult> CreateTeacherAsync(
        [FromBody] CreateTeacherRequest request,
        [FromServices] CreateTeacherCommand createTeacherCommand,
        HttpContext ctx
    )
    {
        var createTeacherPayload = new CreateTeacherCommandPayload
        {
            TeacherGuid = request.TeacherGuid,
            FullName = request.FullName,
            Permissions = TeacherPermissions.DefaultAccess,
        };

        var result = await createTeacherCommand.ExecuteAsync(createTeacherPayload);

        return result.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> GivePermissionsToTeacherAsync(
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
                TeacherPermissions.SuperUser
            );
        }
        else
        {
            await permissionValidator.ValidateTeacherPermissionsAndThrow(
                callerGuid,
                TeacherPermissions.AdminAccess
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
