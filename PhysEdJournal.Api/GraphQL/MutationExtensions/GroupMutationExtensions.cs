using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.GroupExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class GroupMutationExtensions
{
    public async Task<Success> AssignCuratorToGroup(string groupName, string teacherGuid, 
        [Service] IGroupService groupService)
    {
        var res = await groupService.AssignCuratorAsync(groupName, teacherGuid);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(NullVisitValueException))]
    public async Task<Success> AssignVisitValue(string groupName, double newVisitValue,
        [Service] IGroupService groupService)
    {
        var res = await groupService.AssignVisitValueAsync(groupName, newVisitValue);

        return res.Match(_ => true, exception => throw exception);
    }
    
    public async Task<Success> UpdateGroupsInfo([Service] IGroupService groupService)
    {
        var res = await groupService.UpdateGroupsInfoAsync();

        return res.Match(_ => true, exception => throw exception);
    }
}