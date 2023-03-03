using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly ApplicationContext _applicationContext;

    public TeacherService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
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
            return new Result<Unit>(err);
        }
    }
}