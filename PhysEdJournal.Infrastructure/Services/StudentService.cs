using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly ApplicationContext _applicationContext;
    private readonly SemesterEntity _currentSemester;
    private readonly int POINT_AMOUNT; // Кол-во баллов для получения зачета
    
    public StudentService(ApplicationContext applicationContext, TxtFileConfig fileConfig, IConfiguration configuration)
    {
        _applicationContext = applicationContext;
        _currentSemester = SemesterEntity.FromString(fileConfig.ReadTextFromFile()); // Чтобы была возможность выставлять баллы и архивировать студентов на основе текущего семестра
        int.TryParse(configuration["PointBorderForSemester"], out POINT_AMOUNT); 
    }

    public async Task<Result<StudentPointsHistoryEntity>> AddPointsAsync(string studentGuid, string teacherGuid, int pointsAmount, DateOnly date, WorkType workType, string? comment = null)
    {
        var student = await _applicationContext.Students.FindAsync(studentGuid);

        if (student is null)
        {
            return await Task.FromResult(new Result<StudentPointsHistoryEntity>(new StudentNotFound(studentGuid)));
        }
        
        student.AdditionalPoints += pointsAmount;

        var record = new StudentPointsHistoryEntity
        {
            TeacherGuid = teacherGuid, 
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            StudentGuid = studentGuid,
            Comment = comment,
            SemesterId = _currentSemester.Id
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

    public async Task<Result<ArchivedStudentEntity>> ArchiveStudent(string studentGuid)
    {
        var student = await _applicationContext.Students
            .AsNoTracking()
            .Where(s => s.StudentGuid == studentGuid)
            .Select(s => new {s.Group.VisitValue, s.Visits, s.AdditionalPoints, s.FullName, s.GroupNumber, s.HasDebtFromPreviousSemester, s.ArchivedVisitValue})
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new Result<ArchivedStudentEntity>(new StudentNotFound(studentGuid));
        }
        
        if ((student.Visits * student.VisitValue + student.AdditionalPoints) < POINT_AMOUNT) // если не превысил порог по баллам
        {
            await _applicationContext.Students
                .Where(s => s.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.HasDebtFromPreviousSemester, true)
                    .SetProperty(s => s.ArchivedVisitValue, student.VisitValue));
            return new Result<ArchivedStudentEntity>(new NotEnoughPoints(studentGuid));
        }
        
        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = studentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = student.Visits * student.VisitValue + student.AdditionalPoints,
            SemesterId = _currentSemester.Id,
            Visits = student.Visits
        };

        await _applicationContext.ArchivedStudents.AddAsync(archivedStudent);

        await _applicationContext.SaveChangesAsync();

        // Удалить историю с прошлого семестра
        // await _applicationContext.StudentsPointsHistory
        //     .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
        //     .ExecuteDeleteAsync();
        
        await _applicationContext.StudentsVisitsHistory
            .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
            .ExecuteDeleteAsync();

        
        
        // Заархивировать историю с текущего семестра
        await _applicationContext.StudentsPointsHistory
            .Where(h => h.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p
                .SetProperty(s => s.IsArchived, true));
        
        await _applicationContext.StudentsVisitsHistory
            .Where(h => h.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p
                .SetProperty(s => s.IsArchived, true));

        await _applicationContext.Students
            .Where(s => s.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p
                .SetProperty(s => s.AdditionalPoints, 0)
                .SetProperty(s => s.Visits, 0)
                .SetProperty(s => s.HasDebtFromPreviousSemester, false)
                .SetProperty(s => s.ArchivedVisitValue, 0));
        
        return archivedStudent;
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