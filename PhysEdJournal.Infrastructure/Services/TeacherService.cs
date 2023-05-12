using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;
    private readonly IMemoryCache _memoryCache;

    public TeacherService(ApplicationContext applicationContext, PermissionValidator permissionValidator, IMemoryCache memoryCache)
    {
        _applicationContext = applicationContext;
        _permissionValidator = permissionValidator;
        _memoryCache = memoryCache;
    }
    
    public async Task<Result<TeacherEntity>> GivePermissionsAsync(string callerGuid, string teacherGuid, TeacherPermissions type)
    {
        if (type == TeacherPermissions.SuperUser)
            return new Result<TeacherEntity>(new CannotGrantSuperUserPermissionsException(teacherGuid));
        
        try
        {
            var requiredPermissions = type == TeacherPermissions.AdminAccess
                ? FOR_ONLY_SUPERUSER_USE_PERMISSIONS
                : FOR_ONLY_ADMIN_USE_PERMISSIONS;
            
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, requiredPermissions);
            
            var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

            if (teacher is null)
            {
                return await Task.FromResult(new Result<TeacherEntity>(new TeacherNotFoundException(teacherGuid)));
            }

            teacher.Permissions = type;
            
            using var entry = _memoryCache.CreateEntry(teacherGuid);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.Value = teacher;
            
            _applicationContext.Teachers.Update(teacher);
            await _applicationContext.SaveChangesAsync();

            return teacher;
        }
        catch (Exception e)
        {
            return new Result<TeacherEntity>(e);
        }
    }

    public async Task<Result<Unit>> CreateTeacherAsync(string callerGuid, TeacherEntity teacherEntity)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            var teacherGuid = await _applicationContext.Teachers
                .AsNoTracking()
                .Where(t => t.TeacherGuid == teacherEntity.TeacherGuid)
                .Select(t => t.TeacherGuid)
                .FirstOrDefaultAsync();

            if (teacherGuid is not null)
                return new Result<Unit>(new TeacherAlreadyExistsException(teacherGuid));
            
            await _applicationContext.Teachers.AddAsync(teacherEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> CreateCompetitionAsync(string callerGuid, string competitionName)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            var comp = new CompetitionEntity{CompetitionName = competitionName};

            _applicationContext.Competitions.Add(comp);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> DeleteCompetitionAsync(string callerGuid, string competitionName)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            var comp = await _applicationContext.Competitions.FindAsync(competitionName);

            if (comp is null)
                return new Result<Unit>(new CompetitionNotFoundException(competitionName));

            _applicationContext.Competitions.Remove(comp);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}