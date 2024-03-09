using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Group.Contracts;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Rest.Group;

public static class GroupController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(GroupErrors.Errors);

        router
            .MapPost("/AssignCurator", AssignCuratorToGroup)
            .AddEndpointFilter<
                ValidationFilter<AssignCuratorToGroupRequest, AssignCuratorToGroupRequest.Validator>
            >();
        router
            .MapPost("/AssignVisitValue", AssignVisitValue)
            .AddEndpointFilter<
                ValidationFilter<AssignVisitValueRequest, AssignVisitValueRequest.Validator>
            >();
    }

    public static async Task<IResult> AssignCuratorToGroup(
        [FromBody] AssignCuratorToGroupRequest request,
        [FromServices] AssignCuratorCommand assignCuratorCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var assignCuratorPayload = new AssignCuratorCommandPayload
        {
            GroupName = request.GroupName,
            TeacherGuid = request.TeacherGuid,
        };

        var res = await assignCuratorCommand.ExecuteAsync(assignCuratorPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    public static async Task<IResult> AssignVisitValue(
        [FromBody] AssignVisitValueRequest request,
        [FromServices] AssignVisitValueCommand assignVisitValueCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var assignVisitValuePayload = new AssignVisitValueCommandPayload
        {
            GroupName = request.GroupName,
            NewVisitValue = request.NewVisitValue,
        };

        var res = await assignVisitValueCommand.ExecuteAsync(assignVisitValuePayload);

        return res.Match(Response.Ok, Response.Error);
    }
}
