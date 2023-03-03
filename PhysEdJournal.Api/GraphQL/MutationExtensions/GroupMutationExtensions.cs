using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.GroupExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GroupMutationExtensions
{
    public async Task<Success> AssignCuratorToGroup(string groupName, string teacherGuid, 
        [Service] IGroupService groupService, [Service] ILogger<IGroupService> logger)
    {
        var res = await groupService.AssignCuratorAsync(groupName, teacherGuid);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during assigning curator with guid: {teacherGuid} to group with name: {groupName}", teacherGuid, groupName);
            throw exception;
        });
    }

    [Error(typeof(NullVisitValueException))]
    public async Task<Success> AssignVisitValue(string groupName, double newVisitValue,
        [Service] IGroupService groupService, [Service] ILogger<IGroupService> logger)
    {
        var res = await groupService.AssignVisitValueAsync(groupName, newVisitValue);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during setting group's visit value {newVisitValue} to group with name: {groupName}", newVisitValue, groupName);
            throw exception;
        });
    }
    
    public async Task<Success> UpdateGroupsInfo([Service] IGroupService groupService, [Service] ILogger<IGroupService> logger)
    {
        var res = await groupService.UpdateGroupsInfoAsync();
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating groups' info in database");
            throw exception;
        });
    }
}