using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.TeacherServiceFunctions;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly ILogger<TeacherService> _logger;
    private readonly ApplicationContext _applicationContext;
    private readonly int _pageSize;
    private readonly string _userInfoServerURL;

    public TeacherService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options, ILogger<TeacherService> logger)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _userInfoServerURL = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> UpdateTeacherInfoAsync()
    {
        try
        {
            var getTeachersTask = GetAllTeachersAsync(_userInfoServerURL, pageSize: _pageSize).ToListAsync();
            var getDbTeachersTask = _applicationContext.Teachers.ToDictionaryAsync(t => t.TeacherGuid);

            await Task.WhenAll(getTeachersTask.AsTask(), getDbTeachersTask);

            var dbTeachers = getDbTeachersTask.Result;
            var newTeachers = getTeachersTask.Result
                .Where(t => !dbTeachers.ContainsKey(t.Guid))
                .Select(t => new TeacherEntity
                {
                    TeacherGuid = t.Guid, 
                    FullName = t.FullName, 
                    Permissions = TeacherPermissions.DefaultAccess
                });
        
            _applicationContext.Teachers.AddRange(newTeachers);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during updating teachers' info in database");
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<TeacherEntity>> GivePermissionsAsync(string teacherGuid, TeacherPermissions type)
    {
        try
        {
            var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

            if (teacher is null)
            {
                return await Task.FromResult(new Result<TeacherEntity>(new TeacherNotFoundException(teacherGuid)));
            }

            teacher.Permissions = type;
            _applicationContext.Teachers.Update(teacher);
            await _applicationContext.SaveChangesAsync();

            return teacher;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during updating teacher's permissions. Teacher guid: {teacherGuid}", teacherGuid);
            return new Result<TeacherEntity>(e);
        }
    }

    public async Task<Result<Unit>> CreateTeacherAsync(TeacherEntity teacherEntity)
    {
        try
        {
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
            _logger.LogError(err, "Error during teacher creation. Teacher: {teacherEntity}", teacherEntity);
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<TeacherEntity?>> GetTeacherAsync(string guid)
    {
        try
        {
            var teacher = await _applicationContext.Teachers.FindAsync(guid);
            return teacher;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<TeacherEntity?>(err);
        }
    }

    public async Task<Result<Unit>> UpdateTeacherAsync(TeacherEntity updatedTeacher)
    {
        try
        {
            var teacher = await _applicationContext.Teachers.FindAsync(updatedTeacher.TeacherGuid);

            teacher = updatedTeacher;

            _applicationContext.Teachers.Update(teacher);
            await _applicationContext.SaveChangesAsync();
            
            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> DeleteTeacherAsync(string guid)
    {
        try
        {
            var teacherFromDb = await GetTeacherAsync(guid);

            return await teacherFromDb.Match<Task<Result<Unit>>>(async teacher =>
                {
                    _applicationContext.Teachers.Remove(teacher!);
                    await _applicationContext.SaveChangesAsync();
                
                    return Unit.Default;
                },
                exception => Task.FromResult(new Result<Unit>(exception)));
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<Unit>(err);
        }
    }
}