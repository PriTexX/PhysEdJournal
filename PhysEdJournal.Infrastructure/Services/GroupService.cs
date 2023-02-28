using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class GroupService : IGroupService
{
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerURL;
    private readonly int _pageSize;

    public GroupService(ApplicationContext applicationContext, IConfiguration configuration)
    {
        _applicationContext = applicationContext;
        _userInfoServerURL = configuration["UserInfoServerURL"] ?? throw new Exception("Specify UserinfoServerURL in config");
        if (!int.TryParse(configuration["PageSizeToQueryUserInfoServer"], out _pageSize))
        {
            throw new Exception("Specify PageSizeToQueryUserInfoServer value in config");
        }
    }

    public async Task<Result<Unit>> AssignCuratorAsync(string groupName, string teacherGuid)
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

    public async Task<Result<Unit>> AssignVisitValueAsync(string groupName, double newVisitValue)
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

    public async Task<Result<Unit>> UpdateGroupsInfoAsync()
    {
        var distinctGroups = await GetAllStudentsAsync(_userInfoServerURL, pageSize: _pageSize)
            .Select(s => s.Group)
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .ToListAsync();

        var dbGroups = await _applicationContext.Groups.ToDictionaryAsync(g => g.GroupName);

        var newGroups = distinctGroups
            .Where(g => !dbGroups.ContainsKey(g))
            .Select(g => new GroupEntity { GroupName = g });

        _applicationContext.Groups.AddRange(newGroups);
        await _applicationContext.SaveChangesAsync();
        
        return Unit.Default;
    }
    
    public async Task<Result<GroupEntity?>> GetExistingGroupOrNewWithName(string groupName)
    {
        var group = await _applicationContext.Groups.FindAsync(groupName);
        
        if (group != null)
        {
            return group;
        }

        group = new GroupEntity()
        {
            GroupName = groupName,
            Students = new List<StudentEntity>(),
            Curator = null,
            CuratorGuid = null
        };

        await CreateGroupAsync(group);

        return group;
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
            var group = await _applicationContext.Groups.FindAsync(updatedGroup.GroupName);

            group = updatedGroup;

            _applicationContext.Groups.Update(group);
            await _applicationContext.SaveChangesAsync();
            
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