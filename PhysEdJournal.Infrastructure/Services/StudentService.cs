using System.Net.Mime;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService
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
            var exception = new Exception("Нет пользователя");
            return await Task.FromResult(new Result<StudentPointsHistoryEntity>(exception));
        }
        
        student.PointsAmount += pointsAmount;

        var record = new StudentPointsHistoryEntity()
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

    public void AddVisits(float visitsAmount, string comment = null)
    {
        
    }

    internal async Task<Result<Unit>> CreateAsync(StudentEntity studentEntity)
    { 
        _applicationContext.Students.Add(studentEntity);
        await _applicationContext.SaveChangesAsync();
        
        return Unit.Default;
    }

    public async Task<Result<StudentEntity?>> GetAsync(string guid)
    {
        var student = await _applicationContext.Students.FindAsync(guid);

        return student;
    }

    public async Task<Result<Unit>> UpdateAsync(string guid)
    {
        var studentFromDb = await GetAsync(guid);

        return await studentFromDb.Match<Task<Result<Unit>>>(async student =>
            {
                _applicationContext.Students.Update(student!);
                await _applicationContext.SaveChangesAsync();
                
                return Unit.Default;
            },
            exception => Task.FromResult(new Result<Unit>(exception)));
    }

    public async Task<Result<Unit>> DeleteAsync(string guid)
    {
        var studentFromDb = await GetAsync(guid);

        return await studentFromDb.Match<Task<Result<Unit>>>(async student =>
            {
                _applicationContext.Students.Remove(student!);
                await _applicationContext.SaveChangesAsync();
                
                return Unit.Default;
            },
            exception => Task.FromResult(new Result<Unit>(exception)));
    }
}