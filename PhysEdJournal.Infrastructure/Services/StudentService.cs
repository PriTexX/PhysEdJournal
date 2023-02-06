using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly ApplicationContext _applicationContext;
    
    public StudentService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<StudentPointsHistoryEntity>> AddPointsAsync(int pointsAmount, DateOnly date, WorkType workType, string studentGuid, string? comment = null)
    {
        var student = await _applicationContext.Students.FindAsync(studentGuid);

        if (student is null)
        {
            return await Task.FromResult(new Result<StudentPointsHistoryEntity>(new Exception($"No student with such guid {studentGuid}")));
        }
        
        student.AdditionalPoints += pointsAmount;

        var record = new StudentPointsHistoryEntity
        {
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            StudentGuid = studentGuid,
            Comment = comment
        };

        await _applicationContext.StudentsPointsHistory.AddAsync(record);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        return record;
    }

    public async Task<Result<StudentVisitsHistoryEntity>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(studentGuid);

            if (student is null)
            {
                return await Task.FromResult(new Result<StudentVisitsHistoryEntity>(new Exception($"No student with such guid {studentGuid}")));
            }
        
            student.Visits++;

            var record = new StudentVisitsHistoryEntity
            {
                Date = date,
                StudentGuid = studentGuid,
                TeacherGuid = teacherGuid
            };

            await _applicationContext.StudentsVisitsHistory.AddAsync(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return record;
        }
        catch (Exception err)
        {
            return new Result<StudentVisitsHistoryEntity>(err);
        }
    }

    public async Task<Result<Unit>> CreateStudentAsync(StudentEntity studentEntity)
    {
        try
        {
            await _applicationContext.Students.AddAsync(studentEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<StudentEntity?>> GetStudentAsync(string guid)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(guid);
            return student;
        }
        catch (Exception err)
        {
            return new Result<StudentEntity?>(err);
        }
    }

    public async Task<Result<Unit>> UpdateStudentAsync(string guid, StudentEntity updatedStudent)
    {
        try
        {
            _applicationContext.Students.Update(updatedStudent);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> DeleteStudentAsync(string guid)
    {
        try
        {
            var studentFromDb = await GetStudentAsync(guid);

            return await studentFromDb.Match<Task<Result<Unit>>>(async student =>
                {
                    _applicationContext.Students.Remove(student!);
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