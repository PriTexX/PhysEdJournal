using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Controllers;

[Route("api/[controller][action]")]
public sealed class GroupController : Controller
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCuratorToGroup(
        string groupName,
        string teacherGuid,
        [Service] AssignCuratorCommand assignCuratorCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var assignCuratorPayload = new AssignCuratorCommandPayload
        {
            GroupName = groupName,
            TeacherGuid = teacherGuid,
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

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NullVisitValueException))]
    public async Task<IActionResult> AssignVisitValue(
        string groupName,
        double newVisitValue,
        [Service] AssignVisitValueCommand assignVisitValueCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var assignVisitValuePayload = new AssignVisitValueCommandPayload
        {
            GroupName = groupName,
            NewVisitValue = newVisitValue,
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
