using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GroupMutationExtensions
{
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(GroupNotFoundException))]
    public async Task<Success> AssignCuratorToGroup(
        string groupName, string teacherGuid, 
        [Service] AssignCuratorCommand assignCuratorCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var assignCuratorPayload = new AssignCuratorCommandPayload
        {
            GroupName = groupName,
            TeacherGuid = teacherGuid
        };
        
        var res = await assignCuratorCommand.ExecuteAsync(assignCuratorPayload);
        
        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NullVisitValueException))]
    public async Task<Success> AssignVisitValue(
        string groupName, double newVisitValue,
        [Service] AssignVisitValueCommand assignVisitValueCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var assignVisitValuePayload = new AssignVisitValueCommandPayload
        {
            GroupName = groupName,
            NewVisitValue = newVisitValue
        };
        
        var res = await assignVisitValueCommand.ExecuteAsync(assignVisitValuePayload);

        return res.Match(_ => true, exception => throw exception);
    }

    private static void ThrowIfCallerGuidIsNull(string? callerGuid)
    {
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }
    }
}