using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GroupMutationExtensions
{
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(GroupNotFoundException))]
    public async Task<Success> AssignCuratorToGroup(
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
            TeacherPermissions.AdminAccess
        );

        var assignCuratorPayload = new AssignCuratorCommandPayload
        {
            GroupName = groupName,
            TeacherGuid = teacherGuid,
        };

        var res = await assignCuratorCommand.ExecuteAsync(assignCuratorPayload);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(WrongVisitValueException))]
    public async Task<Success> AssignVisitValue(
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
            TeacherPermissions.AdminAccess
        );

        var assignVisitValuePayload = new AssignVisitValueCommandPayload
        {
            GroupName = groupName,
            NewVisitValue = newVisitValue,
        };

        var res = await assignVisitValueCommand.ExecuteAsync(assignVisitValuePayload);

        return res.Match(_ => true, exception => throw exception);
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
