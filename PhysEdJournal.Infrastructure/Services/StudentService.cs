using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
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
    private readonly int _pointAmount; // Кол-во баллов для получения зачета
    
    public StudentService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options, IGroupService groupService, ILogger<StudentService> logger)
    {
        _logger = logger;
        _groupService = groupService;
        _applicationContext = applicationContext;
        _pointAmount = options.Value.PointBorderForSemester;
        _userInfoServerURL = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> AddPointsAsync(PointsStudentHistoryEntity pointsStudentHistoryEntity)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(pointsStudentHistoryEntity.StudentGuid);

        if (student is null)
        {
            return await Task.FromResult(new Result<Unit>(new StudentNotFound(pointsStudentHistoryEntity.StudentGuid)));
        }

        student.AdditionalPoints += pointsStudentHistoryEntity.Points;
        
        _applicationContext.StudentsPointsHistory.Add(pointsStudentHistoryEntity);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(studentGuid);

            if (student is null)
            {
                return await Task.FromResult(new Result<Unit>(new StudentNotFound(studentGuid)));
            }
        
            student.Visits++;

            var record = new VisitStudentHistoryEntity
            {
                Date = date,
                StudentGuid = studentGuid,
                TeacherGuid = teacherGuid
            };

            _applicationContext.StudentsVisitsHistory.Add(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err, "Error during visit increase on student with guid: {studentGuid} and teacher guid: {teacherGuid}", studentGuid, teacherGuid);
            return new Result<Unit>(err);
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
                (student.Visits * student.VisitValue + student.AdditionalPoints) > _pointAmount) // если превысил порог по баллам
            {
                return await Archive(studentGuid, student.FullName, student.GroupNumber, student.Visits, student.VisitValue, student.AdditionalPoints, currentSemesterName);
            }

            await _applicationContext.Students
                .Where(s => s.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.HasDebtFromPreviousSemester, true)
                    .SetProperty(s => s.ArchivedVisitValue, student.VisitValue));
            return new Result<ArchivedStudentEntity>(new NotEnoughPointsException(studentGuid));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during archiving. Student guid: {studentGuid}", studentGuid);
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

            var res = await ArchiveCurrentSemesterHistory(studentGuid);

            return res.Match(_ => archivedStudent, exception => new Result<ArchivedStudentEntity>(exception));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during archiving. Student guid: {studentGuid}", studentGuid);
            return new Result<ArchivedStudentEntity>(e);
        }
    }

    private async Task<Result<Unit>> ArchiveCurrentSemesterHistory(string studentGuid)
    {
        try
        {

            await _applicationContext.StudentsVisitsHistory
                .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
                .ExecuteDeleteAsync();
        
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
            
            return Unit.Default;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during archiving student's semester history. Student guid: {studentGuid}", studentGuid);
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> UpdateStudentsInfoAsync()
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
            _logger.LogError(e, "Error during updating students' info in database");
            return new Result<Unit>(e);
        }
    }
}