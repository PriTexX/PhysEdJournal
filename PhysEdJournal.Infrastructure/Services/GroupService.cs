using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class GroupService : IGroupService
{
    private readonly ApplicationContext _applicationContext;

    public GroupService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> AssignCurator(string groupName, string teacherGuid)
    {
        var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);
        
        if (teacher == null)
        {
            return new Result<Unit>(new Exception("No such teacher found"));
        }
        
        var group = await _applicationContext.Groups.FindAsync(groupName);
        
        if (group == null)
        {
            return new Result<Unit>(new Exception("No such group found"));
        }

        group.Curator = teacher;
        group.CuratorGuid = teacher.TeacherGuid;
        
        await UpdateGroupAsync(group);

        return Unit.Default;
    }

    public async Task<Result<Unit>> AssignVisitValue(string groupName, double newVisitValue)
    {
        var group = await _applicationContext.Groups.FindAsync(groupName);

        if (group == null)
        {
            return new Result<Unit>(new Exception("No such group found"));
        }

        group.VisitValue = newVisitValue;

        await UpdateGroupAsync(group);

        return Unit.Default;
    }

    public async Task<Result<Unit>> CreateGroupAsync(GroupEntity groupEntity)
    {
        try
        {
            await _applicationContext.Groups.AddAsync(groupEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<GroupEntity?>> GetGroupAsync(string groupName)
    {
        try
        {
            var group = await _applicationContext.Groups.FindAsync(groupName);
            return group;
        }
        catch (Exception err)
        {
            return new Result<GroupEntity?>(err);
        }
    }

    public async Task<Result<Unit>> UpdateGroupAsync(GroupEntity updatedGroup)
    {
        try
        {
            await _applicationContext.Groups.Where(g => g.GroupName == updatedGroup.GroupName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(g => g.Students, updatedGroup.Students)
                    .SetProperty(g => g.VisitValue, updatedGroup.VisitValue)
                    .SetProperty(g => g.Curator, updatedGroup.Curator)
                    .SetProperty(g => g.CuratorGuid, updatedGroup.CuratorGuid));

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> DeleteGroupAsync(string groupName)
    {
        try
        {
            await _applicationContext.Groups.Where(g => g.GroupName == groupName).ExecuteDeleteAsync();
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}