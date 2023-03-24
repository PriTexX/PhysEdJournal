using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GroupMutationExtensions
{
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> AssignCuratorToGroup(string groupName, string teacherGuid, 
        [Service] IGroupService groupService, [Service] ILogger<IGroupService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        var res = await groupService.AssignCuratorAsync(callerGuid, groupName, teacherGuid);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during assigning curator with guid: {teacherGuid} to group with name: {groupName}", teacherGuid, groupName);
            throw exception;
        });
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NullVisitValueException))]
    public async Task<Success> AssignVisitValue(string groupName, double newVisitValue,
        [Service] IGroupService groupService, [Service] ILogger<IGroupService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        var res = await groupService.AssignVisitValueAsync( callerGuid, groupName, newVisitValue);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during setting group's visit value {newVisitValue} to group with name: {groupName}", newVisitValue, groupName);
            throw exception;
        });
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> UpdateGroupsInfo([Service] IGroupService groupService, [Service] ILogger<IGroupService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        var res = await groupService.UpdateGroupsInfoAsync(callerGuid);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating groups' info in database");
            throw exception;
        });
    }
}