using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
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
        [Service] AddPointsCommand addPointsCommand,
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

        var addPointsPayload = new AddPointsCommandPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            Comment = comment,
        };
        
        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(_ => true, exception => throw exception);
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
        [Service] AddStandardPointsCommand addStandardPointsCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, TeacherPermissions.DefaultAccess);
        
        var addPointsForStandardPayload = new AddStandardPointsCommandPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            StandardType = standardType,
        };
        var res = await addStandardPointsCommand.ExecuteAsync(addPointsForStandardPayload);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(VisitExpiredException))]
    [Error(typeof(VisitAlreadyExistsException))]
    [Error(typeof(ActionFromFutureException))]
    public async Task<Success> IncreaseStudentVisits(
        string studentGuid, DateOnly date,  
        [Service] IncreaseStudentVisitsCommand increaseStudentVisitsCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, TeacherPermissions.DefaultAccess);

        var increaseStudentVisitsPayload = new IncreaseStudentVisitsCommandPayload
        {
            Date = date,
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid
        };
        
        var res = await increaseStudentVisitsCommand.ExecuteAsync(increaseStudentVisitsPayload);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPointsException))]
    [Error(typeof(CannotMigrateToNewSemesterException))]
    public async Task<ArchivedStudentEntity> ArchiveStudent(
        [Service] ArchiveStudentCommand archiveStudentCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid, bool isForceMode = false)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var archiveStudentPayload = new ArchiveStudentCommandPayload
        {
            StudentGuid = studentGuid,
            IsForceMode = isForceMode
        };
        
        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(archivedStudent => archivedStudent, exception => throw exception);
    }

    public async Task<Success> UnArchiveStudent(
        string studentGuid, string semesterName,
        [Service] UnArchiveStudentCommand unArchiveStudentCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var unArchiveStudentPayload = new UnArchiveStudentCommandPayload
        {
            StudentGuid = studentGuid,
            SemesterName = semesterName
        };
        
        var res = await unArchiveStudentCommand.ExecuteAsync(unArchiveStudentPayload);
        
        return res.Match(_ => true, exception => throw exception);
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> UpdateStudentsInfo(
        [Service] UpdateStudentsInfoCommand updateStudentsInfoCommand, 
        [Service] ILogger<StudentService> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        Task.Run(async () =>
        {
            try
            {
                logger.LogInformation("Teacher: {teacherGuid} started {commandName}", callerGuid, nameof(UpdateStudentsInfoCommand));
                
                await updateStudentsInfoCommand.ExecuteAsync(EmptyPayload.Empty);
                
                logger.LogInformation("{commandName} has successfully finished", nameof(UpdateStudentsInfoCommand));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception happened in {commandName}", nameof(UpdateStudentsInfoCommand));
            }    
        });

        return true;
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StudentNotFoundException))]
    public async Task<Success> ActivateStudent(
        string studentGuid,
        [Service] ActivateStudentCommand activateStudentCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);

        var res = await activateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(_ => true, exception => throw exception);
    }
    
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StudentNotFoundException))]
    public async Task<Success> DeActivateStudent(
        string studentGuid,
        [Service] DeActivateStudentCommand deActivateStudentCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);
        
        await permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
        
        var res = await deActivateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(_ => true, exception => throw exception);
    }

    private static void ThrowIfCallerGuidIsNull(string? callerGuid)
    {
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }
    }
}