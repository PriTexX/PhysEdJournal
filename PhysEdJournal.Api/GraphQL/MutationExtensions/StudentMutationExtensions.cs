using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

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
        WorkType workType, string currentSemesterName,
        string? comment = null)
    {
        var pointsHistory = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            TeacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid"),
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            SemesterName = currentSemesterName,
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
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
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
    public async Task<ArchivedStudentEntity> ArchiveStudent([Service] IStudentService studentService, 
        [Service] ILogger<IStudentService> logger, ClaimsPrincipal claimsPrincipal,
        string studentGuid, string currentSemesterName, bool isForceMode = false)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await studentService.ArchiveStudentAsync(teacherGuid, studentGuid, currentSemesterName, isForceMode);

        return res.Match(archivedStudent => archivedStudent, exception =>
        {
            logger.LogError(exception, "Error during archiving. Student guid: {studentGuid}", studentGuid);
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