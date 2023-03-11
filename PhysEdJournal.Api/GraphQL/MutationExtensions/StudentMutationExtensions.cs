using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Api.Permissions;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using static PhysEdJournal.Api.Permissions.PermissionsConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> AddPointsToStudent([Service] IStudentService studentService, 
        [Service] ILogger<IStudentService> logger, [Service] PermissionValidator permissionValidator,
        string studentGuid, string teacherGuid, 
        int pointsAmount, DateOnly date, 
        WorkType workType, string currentSemesterName,
        string? comment = null)
    {
        if (workType is WorkType.InternalTeam or WorkType.Activist)
        {
            var validationResult = await permissionValidator.ValidateTeacherPermissions(teacherGuid, ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS);
            validationResult.Match(_ => true, exception => throw exception);
        }

        if (workType is WorkType.OnlineWork)
        {
            var validationResult = await permissionValidator.ValidateTeacherPermissions(teacherGuid, ADD_POINTS_FOR_LMS_PERMISSIONS);
            validationResult.Match(_ => true, exception => throw exception);
        }
        
        var pointsHistory = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            TeacherGuid = teacherGuid,
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
    public async Task<Success> IncreaseStudentVisits(string studentGuid, DateOnly date, string teacherGuid, 
        [Service] IStudentService studentService, [Service] ILogger<IStudentService> logger)
    {
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
        [Service] ILogger<IStudentService> logger, 
        string studentGuid, string currentSemesterName, bool isForceMode = false) // TODO добавить аутентификацию и проверить гуид из jwt на наличие прав админа
    {
        var res = await studentService.ArchiveStudentAsync(studentGuid, currentSemesterName, isForceMode);

        return res.Match(archivedStudent => archivedStudent, exception =>
        {
            logger.LogError(exception, "Error during archiving. Student guid: {studentGuid}", studentGuid);
            throw exception;
        });
    }

    public async Task<Success> UpdateStudentsInfo([Service] IStudentService studentService, [Service] ILogger<IStudentService> logger) // TODO добавить аутентификацию и проверить гуид из jwt на наличие прав админа
    {
        var res = await studentService.UpdateStudentsInfoAsync();

        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating students' info in database");
            throw exception;
        });
    }
}