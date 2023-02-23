using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Application.Services;

public interface IGroupService
{
    public Task<Result<Unit>> AssignCurator(string groupName, string teacherGuid);
    
    public Task<Result<Unit>> AssignVisitValue(string groupName, double newVisitValue);
    
    public Task<Result<Unit>> CreateGroupAsync(GroupEntity groupEntity);
    
    public Task<Result<GroupEntity?>> GetGroupAsync(string groupName);
    
    public Task<Result<Unit>> UpdateGroupAsync(GroupEntity updatedGroup);
    
    public Task<Result<Unit>> DeleteGroupAsync(string groupName);
}