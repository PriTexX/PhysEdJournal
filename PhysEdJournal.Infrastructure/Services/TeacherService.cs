using LanguageExt;
using LanguageExt.Common;
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

    public async Task<Result<TeacherEntity>> GivePermissions(string teacherGuid, TeacherPermissions type)
    {
        try
        {
            var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

            if (teacher is null)
            {
                return await Task.FromResult(new Result<TeacherEntity>(new TeacherNotFound(teacherGuid)));
            }

            teacher.Permissions = type;
            _applicationContext.Teachers.Update(teacher);
            await _applicationContext.SaveChangesAsync();

            return teacher;
        }
        catch (Exception err)
        {
            return new Result<TeacherEntity>(err);
        }
    }

    public async Task<Result<Unit>> CreateTeacherAsync(TeacherEntity teacherEntity)
    {
        try
        {
            await _applicationContext.Teachers.AddAsync(teacherEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
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
            return new Result<TeacherEntity?>(err);
        }
    }

    public async Task<Result<Unit>> UpdateTeacherAsync(TeacherEntity updatedTeacher)
    {
        try
        {
            _applicationContext.Teachers.Update(updatedTeacher);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
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
            return new Result<Unit>(err);
        }
    }
}