using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using PhysEdJournal.Infrastructure.Validators.Standards;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;
using static PhysEdJournal.Core.Permissions.Constants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly IGroupService _groupService;
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;
    private readonly StandardsValidator _standardsValidator;
    private readonly string _userInfoServerURL;
    private readonly int _pageSize;
    private readonly int _pointAmount; // Кол-во баллов для получения зачета
    
    private const int VISIT_LIFE_DAYS = 7;
    private const int MAX_POINTS_FOR_STANDARDS = 30;
    
    public StudentService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options, IGroupService groupService, PermissionValidator permissionValidator, StandardsValidator standardsValidator)
    {
        _groupService = groupService;
        _permissionValidator = permissionValidator;
        _standardsValidator = standardsValidator;
        _applicationContext = applicationContext;
        _pointAmount = options.Value.PointBorderForSemester;
        _userInfoServerURL = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> AddPointsAsync(PointsStudentHistoryEntity pointsStudentHistoryEntity)
    {
        try
        {
            switch (pointsStudentHistoryEntity.WorkType)
            {
                case WorkType.InternalTeam or WorkType.Activist:
                {
                    await _permissionValidator.ValidateTeacherPermissionsAndThrow(pointsStudentHistoryEntity.TeacherGuid, ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS);
                    break;
                }
                case WorkType.OnlineWork:
                {
                    await _permissionValidator.ValidateTeacherPermissionsAndThrow(pointsStudentHistoryEntity.TeacherGuid, ADD_POINTS_FOR_LMS_PERMISSIONS);
                    break;
                }
            }
            
            var student = await _applicationContext.Students.FindAsync(pointsStudentHistoryEntity.StudentGuid);

            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(pointsStudentHistoryEntity.StudentGuid));
            }

            student.AdditionalPoints += pointsStudentHistoryEntity.Points;
            
            _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid)
    {
        try
        {
           await _permissionValidator.ValidateTeacherPermissionsAndThrow(teacherGuid, INCREASE_VISITS_PERMISSIONS);

           if (date.DayNumber > DateOnly.FromDateTime(DateTime.Now).DayNumber)
           {
               return new Result<Unit>(new DayOfVisitBiggerThanNowException(date));
           }

           if (DateOnly.FromDateTime(DateTime.Now).DayNumber-date.DayNumber > VISIT_LIFE_DAYS)
           {
               return new Result<Unit>(new VisitExpiredException(date));
           }

           var student = await _applicationContext.Students.FindAsync(studentGuid);

            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(studentGuid));
            }
        
            student.Visits++;

            var record = new VisitStudentHistoryEntity
            {
                Date = date,
                StudentGuid = studentGuid,
                TeacherGuid = teacherGuid
            };

            var recordCopy = await _applicationContext.VisitsStudentsHistory.FindAsync(record.Date, record.StudentGuid);

            if (recordCopy is not null)
            {
                return new Result<Unit>(new VisitAlreadyExistsException(date));
            }

            _applicationContext.VisitsStudentsHistory.Add(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> AddPointsForStandardsAsync(StandardStudentHistoryEntity standardStudentHistoryEntity)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(standardStudentHistoryEntity.TeacherGuid, ADD_POINTD_FOR_STANDARDS_PERMISSIONS);
           
            var student = await _applicationContext.Students.FindAsync(standardStudentHistoryEntity.StudentGuid);

            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(standardStudentHistoryEntity.StudentGuid));
            }

            _standardsValidator.ValidateStudentPointsForStandards(standardStudentHistoryEntity.Points, student.PointsForStandards, student.StudentGuid);
            
            student.PointsForStandards += student.PointsForStandards + standardStudentHistoryEntity.Points > MAX_POINTS_FOR_STANDARDS ? 0 : standardStudentHistoryEntity.Points;
            
            _applicationContext.StandardsStudentsHistory.Add(standardStudentHistoryEntity);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<ArchivedStudentEntity>> ArchiveStudentAsync(string teacherGuid, string studentGuid, string currentSemesterName, bool isForceMode = false)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(teacherGuid, ARCHIVE_PERMISSIONS);
            
            var student = await _applicationContext.Students
                .AsNoTracking()
                .Where(s => s.StudentGuid == studentGuid)
                .Select(s => new {s.Group.VisitValue, s.Visits, s.AdditionalPoints, s.FullName, s.GroupNumber, s.HasDebtFromPreviousSemester, s.ArchivedVisitValue})
                .FirstOrDefaultAsync();

            if (student is null)
            {
                return new Result<ArchivedStudentEntity>(new StudentNotFoundException(studentGuid));
            }

            if (isForceMode || 
                (student.Visits * student.VisitValue + student.AdditionalPoints) > _pointAmount) // если превысил порог по баллам
            {
                return await ArchiveAsync(studentGuid, student.FullName, student.GroupNumber, student.Visits, student.VisitValue, student.AdditionalPoints, currentSemesterName);
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
            return new Result<ArchivedStudentEntity>(e);
        }
    }
    
    private async Task<Result<ArchivedStudentEntity>> ArchiveAsync(string studentGuid, string fullName, string groupNumber, int visitsAmount, double visitValue, int additionalPoints, string currentSemesterName)
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

            var res = await ArchiveCurrentSemesterHistoryAsync(studentGuid);

            return res.Match(_ => archivedStudent, exception => new Result<ArchivedStudentEntity>(exception));
        }
        catch (Exception e)
        {
            return new Result<ArchivedStudentEntity>(e);
        }
    }

    private async Task<Result<Unit>> ArchiveCurrentSemesterHistoryAsync(string studentGuid)
    {
        try
        {

            await _applicationContext.VisitsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
                .ExecuteDeleteAsync();
        
            await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, true));

            await _applicationContext.VisitsStudentsHistory
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
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> UpdateStudentsInfoAsync(string teacherGuid)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(teacherGuid, INCREASE_VISITS_PERMISSIONS);
            
            await _groupService.UpdateGroupsInfoAsync(teacherGuid);
        
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
            return new Result<Unit>(e);
        }
    }
}