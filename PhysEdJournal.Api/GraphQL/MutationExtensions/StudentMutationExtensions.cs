using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Services;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(ActionFromFutureException))]
    public async Task<Success> AddPointsToStudent(
        [Service] StudentService studentService, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid, int pointsAmount, DateOnly date, WorkType workType, string? comment = null)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        switch (workType)
        {
            case WorkType.InternalTeam or WorkType.Activist or WorkType.Competition:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid,
                    ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS);
                break;
            }
            
            case WorkType.OnlineWork:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid,
                    ADD_POINTS_FOR_LMS_PERMISSIONS);
                break;
            }
        }

        var pointsHistory = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            Comment = comment
        };
        var res = await studentService.AddPointsAsync(pointsHistory);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error adding points to student with guid: {studentGuid}. With points history entity: {pointsHistory}", pointsHistory.StudentGuid, pointsHistory);
            throw exception;
        });
    }
    
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(OverAbundanceOfPointsForStudentException))]
    [Error(typeof(NonRegularPointsValueException))]
    [Error(typeof(StandardAlreadyExistsException))]
    [Error(typeof(ActionFromFutureException))]
    public async Task<Success> AddPointsForStandardToStudent(
        string studentGuid, int pointsAmount, DateOnly date, StandardType standardType,
        [Service] StudentService studentService, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, TeacherPermissions.DefaultAccess);
        
        var pointsForStandardHistory = new StandardsStudentHistoryEntity()
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            StandardType = standardType,
        };
        var res = await studentService.AddPointsForStandardsAsync(pointsForStandardHistory);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error adding points for standard to student with guid: {studentGuid}. With points history entity: {pointsHistory}", pointsForStandardHistory.StudentGuid, pointsForStandardHistory);
            throw exception;
        });
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(VisitExpiredException))]
    [Error(typeof(VisitAlreadyExistsException))]
    [Error(typeof(ActionFromFutureException))]
    public async Task<Success> IncreaseStudentVisits(
        string studentGuid, DateOnly date,  
        [Service] StudentService studentService, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, TeacherPermissions.DefaultAccess);

        var res = await studentService.IncreaseVisitsAsync(studentGuid, date, callerGuid);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during visit increase on student with guid: {studentGuid} and teacher guid: {teacherGuid}", studentGuid, callerGuid);
            throw exception;
        });
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPointsException))]
    [Error(typeof(CannotMigrateToNewSemesterException))]
    public async Task<ArchivedStudentEntity> ArchiveStudent(
        [Service] StudentService studentService, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid, bool isForceMode = false)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var res = await studentService.ArchiveStudentAsync(studentGuid, isForceMode);

        return res.Match(archivedStudent => archivedStudent, exception =>
        {
            logger.LogError(exception, "Error during archiving. Student guid: {studentGuid}", studentGuid);
            throw exception;
        });
    }

    public async Task<Success> UnArchiveStudent(
        string studentGuid, string semesterName,
        [Service] StudentService studentService,
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var res = await studentService.UnArchiveStudentAsync(studentGuid, semesterName);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during unarchiving. Student guid: {studentGuid}. Semester name: {semesterName}", studentGuid, semesterName);
            throw exception;
        });
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> UpdateStudentsInfo(
        [Service] StudentService studentService, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var res = await studentService.UpdateStudentsInfoAsync();

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating students' info in database");
            throw exception;
        });
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StudentNotFoundException))]
    public async Task<Success> ActivateStudent(
        string studentGuid,
        [Service] StudentService studentService,
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
        
        var res = await studentService.ActivateStudentAsync(studentGuid);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during activating student with guid: {studentGuid}", studentGuid);
            throw exception;
        });
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StudentNotFoundException))]
    public async Task<Success> DeActivateStudent(
        string studentGuid,
        [Service] StudentService studentService,
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
        
        var res = await studentService.DeActivateStudentAsync(studentGuid);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during deactivating student with guid: {studentGuid}", studentGuid);
            throw exception;
        });
    }

    private static void ThrowIfCallerGuidIsNull(string? callerGuid)
    {
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }
    }
}