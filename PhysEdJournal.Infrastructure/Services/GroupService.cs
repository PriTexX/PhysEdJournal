using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class GroupService
{
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerURL;
    private readonly int _pageSize;

    public GroupService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options)
    {
        _applicationContext = applicationContext;
        _userInfoServerURL = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> AssignCuratorAsync(string groupName, string teacherGuid)
    {
        try
        {
            var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);
        
            if (teacher == null)
            {
                return new Result<Unit>(new TeacherNotFoundException(teacherGuid));
            }
        
            var group = await _applicationContext.Groups.FindAsync(groupName);
        
            if (group == null)
            {
                return new Result<Unit>(new GroupNotFoundException(groupName));
            }

            group.Curator = teacher;
            group.CuratorGuid = teacher.TeacherGuid;

            _applicationContext.Groups.Update(group);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> AssignVisitValueAsync(string groupName, double newVisitValue)
    {
        try
        {
            if (newVisitValue <= 0)
            {
                return new Result<Unit>(new NullVisitValueException(newVisitValue));
            }
            
            var group = await _applicationContext.Groups.FindAsync(groupName);

            if (group == null)
            {
                return new Result<Unit>(new GroupNotFoundException(groupName));
            }

            group.VisitValue = newVisitValue;

            _applicationContext.Groups.Update(group);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> UpdateGroupsInfoAsync()
    {
        try
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
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }
}