using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Requests;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Rest.Controllers;

public static class GroupController
{
    public static async Task<IResult> AssignCuratorToGroup(
        AssignCuratorToGroupRequest request,
        [FromServices] AssignCuratorCommand assignCuratorCommand,
        [FromServices] PermissionValidator permissionValidator
    )
    {
        var callerGuid = GetCallerGuid(request.ClaimsPrincipal);

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
            exception => Results.StatusCode(StatusCodes.Status500InternalServerError)
        );
    }

    public static async Task<IResult> AssignVisitValue(
        AssignVisitValueRequest request,
        [FromServices] AssignVisitValueCommand assignVisitValueCommand,
        [FromServices] PermissionValidator permissionValidator
    )
    {
        var callerGuid = GetCallerGuid(request.ClaimsPrincipal);

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
            exception => Results.StatusCode(StatusCodes.Status500InternalServerError)
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