using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly ILogger<StudentService> _logger;
    private readonly IGroupService _groupService;
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerURL;
    private readonly int _pageSize;
    private readonly int POINT_AMOUNT; // Кол-во баллов для получения зачета
    
    public StudentService(ApplicationContext applicationContext, IConfiguration configuration, IGroupService groupService, ILogger<StudentService> logger)
    {
        _logger = logger;
        _groupService = groupService;
        _applicationContext = applicationContext;
        int.TryParse(configuration["PointBorderForSemester"], out POINT_AMOUNT);
        _userInfoServerURL = configuration["UserInfoServerURL"] ?? throw new Exception("Specify UserinfoServerURL in config");
        if (!int.TryParse(configuration["PageSizeToQueryUserInfoServer"], out _pageSize))
        {
            throw new Exception("Specify PageSizeToQueryUserInfoServer value in config");
        }
    }

    public async Task<Result<PointsStudentHistoryEntity>> AddPointsAsync(string studentGuid, string teacherGuid, int pointsAmount, DateOnly date, WorkType workType, string currentSemesterName, string? comment = null)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(studentGuid);

            if (student is null)
            {
                return await Task.FromResult(new Result<PointsStudentHistoryEntity>(new StudentNotFound(studentGuid)));
            }
        
            student.AdditionalPoints += pointsAmount;

            var record = new PointsStudentHistoryEntity
            {
                TeacherGuid = teacherGuid, 
                Points = pointsAmount,
                SemesterName = currentSemesterName,
                Date = date,
                WorkType = workType,
                StudentGuid = studentGuid,
                Comment = comment,
            };

            _applicationContext.StudentsPointsHistory.Add(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return record;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return new Result<PointsStudentHistoryEntity>(e);
        }
    }

    public async Task<Result<VisitsStudentHistoryEntity>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(studentGuid);

            if (student is null)
            {
                return await Task.FromResult(new Result<VisitsStudentHistoryEntity>(new Exception($"No student with such guid {studentGuid}")));
            }
        
            student.Visits++;

            var record = new VisitsStudentHistoryEntity
            {
                Date = date,
                StudentGuid = studentGuid,
                TeacherGuid = teacherGuid
            };

            _applicationContext.StudentsVisitsHistory.Add(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return record;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<VisitsStudentHistoryEntity>(err);
        }
    }

    public async Task<Result<ArchivedStudentEntity>> ArchiveStudentAsync(string studentGuid, string currentSemesterName, bool isForceMode = false)
    {
        try
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

            if (isForceMode || 
                (student.Visits * student.VisitValue + student.AdditionalPoints) > POINT_AMOUNT) // если превысил порог по баллам
            {
                return await Archive(studentGuid, student.FullName, student.GroupNumber, student.Visits, student.VisitValue, student.AdditionalPoints, currentSemesterName);
            }

            await _applicationContext.Students
                .Where(s => s.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.HasDebtFromPreviousSemester, true)
                    .SetProperty(s => s.ArchivedVisitValue, student.VisitValue));
            return new Result<ArchivedStudentEntity>(new NotEnoughPoints(studentGuid));
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return new Result<ArchivedStudentEntity>(e);
        }
    }
    
    private async Task<Result<ArchivedStudentEntity>> Archive(string studentGuid, string fullName, string groupNumber, int visitsAmount, double visitValue, int additionalPoints, string currentSemesterName)
    {
        try
        {
            var archivedStudent = new ArchivedStudentEntity
            {
                StudentGuid = studentGuid,
                FullName = fullName,
                GroupNumber = groupNumber,
                TotalPoints = visitsAmount * visitValue + additionalPoints,
                SemesterName = currentSemesterName,
                Visits = visitsAmount
            };

            _applicationContext.ArchivedStudents.Add(archivedStudent);
            await _applicationContext.SaveChangesAsync();

            await _applicationContext.StudentsVisitsHistory
                .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
                .ExecuteDeleteAsync();

            await ArchiveCurrentSemesterHistory(studentGuid);

            return archivedStudent;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return new Result<ArchivedStudentEntity>(e);
        }
    }

    private async Task ArchiveCurrentSemesterHistory(string studentGuid)
    {
        try
        {
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
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            throw;
        }
    }

    public async Task<Result<Unit>> UpdateStudentInfoAsync()
    {
        try
        {
            await _groupService.UpdateGroupsInfoAsync();
        
            const int batchSize = 500;
            var updateTasks = GetAllStudentsAsync(_userInfoServerURL, pageSize: _pageSize)
                .Buffer(batchSize)
                .SelectAwait(async actualStudents => new
                {
                    actualStudents = actualStudents.ToDictionary(s => s.Guid), 
                    dbStudents = (await GetManyStudentsWithManyKeys(_applicationContext, actualStudents
                            .Select(s => s.Guid)
                            .ToArray()))
                        .ToDictionary(d => d.StudentGuid)
                })
                .Select(s => s.actualStudents.Keys
                    .Select(actualStudentGuid => 
                    (
                        s.actualStudents[actualStudentGuid], 
                        s.dbStudents.GetValueOrDefault(actualStudentGuid)
                    )))
                .Select(d => d
                    .Select(s => GetUpdatedOrCreatedStudentEntities(s.Item1, s.Item2)))
                .Select(s => CommitChangesToContext(_applicationContext, s.ToList()));

            await foreach (var updateTask in updateTasks)
            {
                await updateTask;
            }

            return Unit.Default;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return new Result<Unit>(e);
        }
    }
    
    public async Task<Result<Unit>> CreateStudentAsync(StudentEntity studentEntity)
    {
        try
        {
            _applicationContext.Students.Add(studentEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
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
            _logger.LogError(err.ToString());
            return new Result<StudentEntity?>(err);
        }
    }

    public async Task<Result<Unit>> UpdateStudentAsync(StudentEntity updatedStudent)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(updatedStudent.StudentGuid);

            student = updatedStudent;

            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();
            
            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<Unit>(err);
        }
    }
    public async Task<Result<Unit>> DeleteStudentAsync(string guid)
    {
        try
        {
            await _applicationContext.Students.Where(s => s.StudentGuid == guid).ExecuteDeleteAsync();
            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<Unit>(err);
        }
    }
}