using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> AddPointsToStudent([Service] IStudentService studentService, ClaimsPrincipal claimsPrincipal, 
        [Service] ILogger<IStudentService> logger,
        string studentGuid, 
        int pointsAmount, DateOnly date, 
        WorkType workType,
        string? comment = null)
    {
        var pointsHistory = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            TeacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid"),
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
    public async Task<Success> AddPointsForStandardToStudent([Service] IStudentService studentService, ClaimsPrincipal claimsPrincipal, 
        [Service] ILogger<IStudentService> logger,
        string studentGuid, 
        int pointsAmount, DateOnly date, 
        StandardType standardType)
    {
        var pointsForStandardHistory = new StandardStudentHistoryEntity()
        {
            StudentGuid = studentGuid,
            TeacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid"),
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
    [Error(typeof(DayOfVisitBiggerThanNowException))]
    public async Task<Success> IncreaseStudentVisits(string studentGuid, DateOnly date,  
        [Service] IStudentService studentService, 
        [Service] ILogger<IStudentService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await studentService.IncreaseVisitsAsync(studentGuid, date, teacherGuid);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during visit increase on student with guid: {studentGuid} and teacher guid: {teacherGuid}", studentGuid, teacherGuid);
            throw exception;
        });
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPointsException))]
    public async Task<ArchivedStudentEntity> ArchiveStudent([Service] IStudentService studentService, 
        [Service] ILogger<IStudentService> logger, ClaimsPrincipal claimsPrincipal,
        string studentGuid, bool isForceMode = false)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await studentService.ArchiveStudentAsync(teacherGuid, studentGuid, isForceMode);

        return res.Match(archivedStudent => archivedStudent, exception =>
        {
            logger.LogError(exception, "Error during archiving. Student guid: {studentGuid}", studentGuid);
            throw exception;
        });
    }

    public async Task<Success> UnArchiveStudent([Service] IStudentService studentService,
        [Service] ILogger<IStudentService> logger, ClaimsPrincipal claimsPrincipal, 
        string studentGuid, string semesterName)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await studentService.UnArchiveStudentAsync(teacherGuid, studentGuid, semesterName);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during unarchiving. Student guid: {studentGuid}. Semester name: {semesterName}", studentGuid, semesterName);
            throw exception;
        });
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> UpdateStudentsInfo([Service] IStudentService studentService, 
        [Service] ILogger<IStudentService> logger,
        ClaimsPrincipal claimsPrincipal)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await studentService.UpdateStudentsInfoAsync(teacherGuid);

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating students' info in database");
            throw exception;
        });
    }
}