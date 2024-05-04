using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Common.Filters;
using PhysEdJournal.Api.Rest.Group.Contracts;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;

namespace PhysEdJournal.Api.Rest.Group;

public static class GroupController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(GroupErrors.Errors);

        var groupRouter = router.MapGroup("/group");

        groupRouter
            .MapPost("/curator", AssignCuratorToGroup)
            .AddValidation(AssignCuratorToGroupRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);

        groupRouter
            .MapPost("/visit-value", AssignVisitValue)
            .AddValidation(AssignVisitValueRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);
    }

    private static async Task<IResult> AssignCuratorToGroup(
        [FromBody] AssignCuratorToGroupRequest request,
        [FromServices] AssignCuratorCommand assignCuratorCommand,
        [FromServices] PermissionValidator permissionValidator
    )
    {
        var assignCuratorPayload = new AssignCuratorCommandPayload
        {
            GroupName = request.GroupName,
            TeacherGuid = request.TeacherGuid,
        };

        var res = await assignCuratorCommand.ExecuteAsync(assignCuratorPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> AssignVisitValue(
        [FromBody] AssignVisitValueRequest request,
        [FromServices] AssignVisitValueCommand assignVisitValueCommand,
        [FromServices] PermissionValidator permissionValidator
    )
    {
        var assignVisitValuePayload = new AssignVisitValueCommandPayload
        {
            GroupName = request.GroupName,
            NewVisitValue = request.NewVisitValue,
        };

        var res = await assignVisitValueCommand.ExecuteAsync(assignVisitValuePayload);

        return res.Match(Response.Ok, Response.Error);
    }
}
