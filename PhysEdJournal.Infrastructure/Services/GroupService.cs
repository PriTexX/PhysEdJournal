using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class GroupService : IGroupService
{
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;
    private readonly string _userInfoServerURL;
    private readonly int _pageSize;

    public GroupService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options, PermissionValidator permissionValidator)
    {
        _applicationContext = applicationContext;
        _permissionValidator = permissionValidator;
        _userInfoServerURL = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> AssignCuratorAsync(string callerGuid, string groupName, string teacherGuid)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

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

    public async Task<Result<Unit>> AssignVisitValueAsync(string callerGuid, string groupName, double newVisitValue)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            if (newVisitValue < 0)
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

    public async Task<Result<Unit>> UpdateGroupsInfoAsync(string callerGuid)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
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