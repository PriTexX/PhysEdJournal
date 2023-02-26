using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Application.Services;

public interface IGroupService
{
    public Task<Result<Unit>> AssignCuratorAsync(string groupName, string teacherGuid);
    
    public Task<Result<Unit>> AssignVisitValueAsync(string groupName, double newVisitValue);

    public Task<Result<GroupEntity?>> GetExistingGroupOrNewWithName(string groupName);

    public Task<Result<Unit>> UpdateGroupsInfoAsync();
    
    public Task<Result<Unit>> CreateGroupAsync(GroupEntity groupEntity);
    
    public Task<Result<GroupEntity?>> GetGroupAsync(string groupName);
    
    public Task<Result<Unit>> UpdateGroupAsync(GroupEntity updatedGroup);
    
    public Task<Result<Unit>> DeleteGroupAsync(string groupName);
}