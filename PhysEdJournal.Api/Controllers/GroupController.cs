using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Controllers.Requests;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Controllers;

[Route("api/[controller][action]")]
public sealed class GroupController : Controller
{
    public async Task<IActionResult> AssignCuratorToGroup(
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
            _ => StatusCode(StatusCodes.Status200OK),
            exception =>
            {
                if (exception is NotEnoughPermissionsException)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                if (exception is TeacherNotFoundException or GroupNotFoundException)
                {
                    return NotFound();
                }

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        );
    }

    public async Task<IActionResult> AssignVisitValue(
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
            _ => StatusCode(StatusCodes.Status200OK),
            exception =>
            {
                if (exception is NotEnoughPermissionsException)
                {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                if (exception is TeacherNotFoundException)
                {
                    return NotFound();
                }

                if (exception is NullVisitValueException)
                {
                    return BadRequest();
                }

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
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
