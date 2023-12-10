using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.Group.Contracts;
using PhysEdJournal.Api.Controllers;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.Group;

public static class GroupController
{
    public static void MapGroupEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(GroupErrors.Errors);

        router.MapPost("/AssignCurator", AssignCuratorToGroup);
        router.MapPost("/AssignVisitValue", AssignVisitValue);
    }

    public static async Task<IResult> AssignCuratorToGroup(
        [FromBody] AssignCuratorToGroupRequest request,
        [FromServices] AssignCuratorCommand assignCuratorCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = GetCallerGuid(ctx.User);

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

        return res.Match(
            _ => Results.Ok(),
            ErrorHandler.HandleErrorResult
        );
    }

    public static async Task<IResult> AssignVisitValue(
        [FromBody] AssignVisitValueRequest request,
        [FromServices] AssignVisitValueCommand assignVisitValueCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = GetCallerGuid(ctx.User);

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

        return res.Match(
            _ => Results.Ok(),
            ErrorHandler.HandleErrorResult
            );
    }

    private static string GetCallerGuid(ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.Claims.First(c => c.Type == "IndividualGuid").Value;
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }

        return callerGuid;
    }
}