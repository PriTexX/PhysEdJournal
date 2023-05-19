﻿using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Standards;
using static PhysEdJournal.Infrastructure.Services.StaticFunctions.StudentServiceFunctions;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService
{
    private readonly GroupService _groupService;
    private readonly ApplicationContext _applicationContext;
    private readonly StandardsValidator _standardsValidator;
    
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public StudentService(ApplicationContext applicationContext, IOptions<ApplicationOptions> options, GroupService groupService, StandardsValidator standardsValidator)
    {
        _groupService = groupService;
        _standardsValidator = standardsValidator;
        _applicationContext = applicationContext;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> AddPointsAsync(PointsStudentHistoryEntity pointsStudentHistoryEntity)
    {
        try
        {
            if (pointsStudentHistoryEntity.Date > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return new Result<Unit>(new ActionFromFutureException(pointsStudentHistoryEntity.Date));
            }

            var student = await _applicationContext.Students.FindAsync(pointsStudentHistoryEntity.StudentGuid);

            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(pointsStudentHistoryEntity.StudentGuid));
            }

            student.AdditionalPoints += pointsStudentHistoryEntity.Points;

            pointsStudentHistoryEntity.SemesterName = student.CurrentSemesterName;
            _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            await TryArchiveStudentWithDebt(student);

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
            if (date > DateOnly.FromDateTime(DateTime.Now))
            {
                return new Result<Unit>(new ActionFromFutureException(date));
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

            var recordCopy = await _applicationContext.VisitsStudentsHistory
                .Where(v => v.StudentGuid == record.StudentGuid && v.Date == record.Date).FirstOrDefaultAsync();

            if (recordCopy is not null)
            {
                return new Result<Unit>(new VisitAlreadyExistsException(date));
            }

            _applicationContext.VisitsStudentsHistory.Add(record);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();
            
            await TryArchiveStudentWithDebt(student);

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<Unit>> AddPointsForStandardsAsync(StandardsStudentHistoryEntity standardsStudentHistoryEntity)
    {
        try
        {
            if (standardsStudentHistoryEntity.Date > DateOnly.FromDateTime(DateTime.Now))
            {
                return new Result<Unit>(new ActionFromFutureException(standardsStudentHistoryEntity.Date));
            }
           
            var student = await _applicationContext.Students.FindAsync(standardsStudentHistoryEntity.StudentGuid);

            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(standardsStudentHistoryEntity.StudentGuid));
            }

            var allHistory = _applicationContext.StandardsStudentsHistory.ToList();
            if (allHistory.FirstOrDefault(s => s.StandardType == standardsStudentHistoryEntity.StandardType) != null)
            {
                return new Result<Unit>(new StandardAlreadyExistsException(standardsStudentHistoryEntity.StudentGuid,
                    standardsStudentHistoryEntity.StandardType));
            }

            _standardsValidator.ValidateStudentPointsForStandardsAndThrow(standardsStudentHistoryEntity.Points, student.PointsForStandards, student.StudentGuid);
            
            student.PointsForStandards += student.PointsForStandards + standardsStudentHistoryEntity.Points > MAX_POINTS_FOR_STANDARDS ? 0 : standardsStudentHistoryEntity.Points;

            standardsStudentHistoryEntity.SemesterName = student.CurrentSemesterName;
            _applicationContext.StandardsStudentsHistory.Add(standardsStudentHistoryEntity);
            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();
            
            await TryArchiveStudentWithDebt(student);

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }

    public async Task<Result<ArchivedStudentEntity>> ArchiveStudentAsync(string studentGuid, bool isForceMode = false)
    {
        try
        {
            var studentFromDb = await _applicationContext.Students
                .AsNoTracking()
                .Where(s => s.StudentGuid == studentGuid)
                .Select(s => new  {
                    s.Group.VisitValue, 
                    s.Visits, 
                    s.AdditionalPoints, 
                    s.PointsForStandards, 
                    s.FullName, 
                    s.GroupNumber, 
                    s.HasDebtFromPreviousSemester, 
                    s.ArchivedVisitValue, 
                    s.CurrentSemesterName})
                .FirstOrDefaultAsync();

            if (studentFromDb is null)
            {
                return new Result<ArchivedStudentEntity>(new StudentNotFoundException(studentGuid));
            }

            var student = new StudentEntity
            {
                Group = new GroupEntity {VisitValue = studentFromDb.VisitValue},
                Visits = studentFromDb.Visits,
                AdditionalPoints = studentFromDb.AdditionalPoints,
                PointsForStandards = studentFromDb.PointsForStandards,
                FullName = studentFromDb.FullName,
                StudentGuid = studentGuid,
                GroupNumber = studentFromDb.GroupNumber,
                HasDebtFromPreviousSemester = studentFromDb.HasDebtFromPreviousSemester,
                ArchivedVisitValue = studentFromDb.ArchivedVisitValue,
                CurrentSemesterName = studentFromDb.CurrentSemesterName
            };

            var activeSemester = await _applicationContext.GetActiveSemester();

            if (student.CurrentSemesterName == activeSemester.Name)
                return new Result<ArchivedStudentEntity>(new CannotMigrateToNewSemesterException(activeSemester.Name));

            var totalPoints = CalculateTotalPoints(student);
            if (isForceMode || totalPoints > POINT_AMOUNT) // если превысил порог по баллам
            {
                await using var transaction = await _applicationContext.Database.BeginTransactionAsync();
                
                await _applicationContext.Students
                    .Where(s => s.StudentGuid == studentGuid)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(s => s.CurrentSemesterName, activeSemester.Name));
                
                var archiveResult = await ArchiveAsync(studentGuid, student.FullName, student.GroupNumber, student.Visits, totalPoints, student.CurrentSemesterName);

                return await archiveResult.Match<Task<Result<ArchivedStudentEntity>>>(async archivedStudent =>
                {
                    await transaction.CommitAsync();
                    return archivedStudent;
                }, async exception => new Result<ArchivedStudentEntity>(exception));
            }

            await _applicationContext.Students
                .Where(s => s.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.HasDebtFromPreviousSemester, true)
                    .SetProperty(s => s.ArchivedVisitValue, student.Group.VisitValue));
            return new Result<ArchivedStudentEntity>(new NotEnoughPointsException(studentGuid, CalculateTotalPoints(student)));
        }
        catch (Exception e)
        {
            return new Result<ArchivedStudentEntity>(e);
        }
    }
    
    private async Task<Result<ArchivedStudentEntity>> ArchiveAsync(string studentGuid, string fullName, string groupNumber, int visitsAmount, double totalPoints, string semesterName)
    {
        try
        {
            var archivedStudent = new ArchivedStudentEntity
            {
                StudentGuid = studentGuid,
                FullName = fullName,
                GroupNumber = groupNumber,
                TotalPoints = totalPoints,
                SemesterName = semesterName,
                Visits = visitsAmount
            };

            _applicationContext.ArchivedStudents.Add(archivedStudent); 
            await _applicationContext.SaveChangesAsync();

            var res = await ArchiveCurrentSemesterHistoryAsync(studentGuid, semesterName);

            return res.Match(_ => archivedStudent, exception => new Result<ArchivedStudentEntity>(exception));
        }
        catch (Exception e)
        {
            return new Result<ArchivedStudentEntity>(e);
        }
    }

    private async Task<Result<Unit>> ArchiveCurrentSemesterHistoryAsync(string studentGuid, string semesterName)
    {
        try
        {
            await _applicationContext.VisitsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
                .ExecuteDeleteAsync();
        
            await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, true));

            await _applicationContext.VisitsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, true));

            await _applicationContext.StandardsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, true));

            await _applicationContext.Students
                .Where(s => s.StudentGuid == studentGuid)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.AdditionalPoints, 0)
                    .SetProperty(s => s.Visits, 0)
                    .SetProperty(s => s.HasDebtFromPreviousSemester, false)
                    .SetProperty(s => s.ArchivedVisitValue, 0)
                    .SetProperty(s => s.PointsForStandards, 0));
            
            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }
    
    public async Task<Result<Unit>> UnArchiveStudentAsync(string studentGuid, string semesterName)
    {
        try
        {
            var student = await _applicationContext.Students.FindAsync(studentGuid);
            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(studentGuid));
            }
        
            var archivedStudent = await _applicationContext.ArchivedStudents.FindAsync(studentGuid, semesterName);
            if (archivedStudent is null)
            {
                return new Result<Unit>(new ArchivedStudentNotFound(studentGuid, semesterName));
            }

            await using var transaction = await _applicationContext.Database.BeginTransactionAsync();
            
            var pointsStudentHistory = await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ToListAsync();
    
            student.AdditionalPoints = pointsStudentHistory.Aggregate(0, (prev, next) => prev + next.Points);
            
            var standardsStudentHistory = await _applicationContext.StandardsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ToListAsync();
            
            student.PointsForStandards = standardsStudentHistory.Aggregate(0, (prev, next) => prev + next.Points);
    
            student.HasDebtFromPreviousSemester = false;
            student.ArchivedVisitValue = 0;
            student.Visits = archivedStudent.Visits;

            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            await _applicationContext.VisitsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, false));

            await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.SemesterName, student.CurrentSemesterName)
                    .SetProperty(s => s.IsArchived, false));
            
            await _applicationContext.StandardsStudentsHistory
                .Where(h => h.StudentGuid == studentGuid && h.SemesterName == semesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.SemesterName, student.CurrentSemesterName)
                    .SetProperty(s => s.IsArchived, false));

            await transaction.CommitAsync();
            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> UpdateStudentsInfoAsync()
    {
        try
        {
            var res = await _groupService.UpdateGroupsInfoAsync();
            res.Match(_ => true, exception => throw exception);

            var currentSemesterName = (await _applicationContext.GetActiveSemester()).Name;
        
            const int batchSize = 500;
            var updateTasks = GetAllStudentsAsync(_userInfoServerUrl, pageSize: _pageSize)
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
                    .Select(s => GetUpdatedOrCreatedStudentEntities(s.Item1, s.Item2, currentSemesterName)))
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

    public async Task<Result<Unit>> DeActivateStudentAsync(string studentGuid)
    {
        try
        {
            try
            {
                await _applicationContext.Students // TODO убрать try/catch здесь т.к. он не имеет смысла (если при обновлении вылезла ошибка, это не значит, что студента нет)
                    .Where(s => s.StudentGuid == studentGuid)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(s => s.IsActive, false));
            }
            catch (Exception e)
            {
                return new Result<Unit>(new StudentNotFoundException(studentGuid));
            }

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    public async Task<Result<Unit>> ActivateStudentAsync(string studentGuid)
    {
        try
        {
            try
            {
                await _applicationContext.Students
                    .Where(s => s.StudentGuid == studentGuid)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(s => s.IsActive, true));
            }
            catch (Exception e)
            {
                return new Result<Unit>(new StudentNotFoundException(studentGuid));
            }

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new Result<Unit>(e);
        }
    }

    private async Task TryArchiveStudentWithDebt(StudentEntity student)
    {
        var hasDebtAndEnoughPoints = CheckIfStudentHasDebtAndEnoughPoints(student);
        if (hasDebtAndEnoughPoints)
        {
            var res = await ArchiveStudentAsync(student.StudentGuid);
            res.Match(_ => true, exception => throw exception);
        }
    }
    
    private bool CheckIfStudentHasDebtAndEnoughPoints(StudentEntity student)
    {
        if (!student.HasDebtFromPreviousSemester)
            return false;
        
        var totalPoints = CalculateTotalPoints(student);
        return totalPoints >= POINT_AMOUNT;
    }

    private double CalculateTotalPoints(StudentEntity student)
    {
        return student.Visits * student.Group.VisitValue + student.AdditionalPoints +
               student.PointsForStandards;
    }
}