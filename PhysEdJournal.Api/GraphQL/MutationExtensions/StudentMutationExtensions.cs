using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Constants;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(ActionFromFutureException))]
    [Error(typeof(FitnessAlreadyExistsException))]
    [Error(typeof(DateExpiredException))]
    [Error(typeof(PointsExceededLimit))]
    [Error(typeof(NegativePointAmount))]
    [Error(typeof(NonWorkingDayException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> AddPointsToStudent(
        [Service] AddPointsCommand addPointsCommand,
        [Service] PermissionValidator permissionValidator,
        [Service] ILogger<StudentMutationExtensions> logger,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid,
        int pointsAmount,
        DateOnly date,
        WorkType workType,
        string? comment = null
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        switch (workType)
        {
            case WorkType.InternalTeam
            or WorkType.Activist
            or WorkType.Competition:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS
                );
                break;
            }

            case WorkType.OnlineWork:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_LMS_PERMISSIONS
                );
                break;
            }
        }

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsPayload = new AddPointsCommandPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            IsAdmin = isAdminOrSecretary,
            Comment = comment,
        };

        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(
            _ => true,
            exception =>
            {
                logger.LogWarning(exception, "Something bad happened");
                throw exception;
            }
        );
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StandardAlreadyExistsException))]
    [Error(typeof(ActionFromFutureException))]
    [Error(typeof(LoweringTheScoreException))]
    [Error(typeof(NotEnoughPointsForStandardException))]
    [Error(typeof(DateExpiredException))]
    [Error(typeof(NonWorkingDayException))]
    [Error(typeof(PointsOverflowException))]
    [Error(typeof(NegativePointAmount))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> AddPointsForStandardToStudent(
        string studentGuid,
        int pointsAmount,
        DateOnly date,
        StandardType standardType,
        bool isOverride,
        [Service] AddStandardPointsCommand addStandardPointsCommand,
        [Service] PermissionValidator permissionValidator,
        [Service] ILogger<StudentMutationExtensions> logger,
        ClaimsPrincipal claimsPrincipal,
        string? comment = null
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsForStandardPayload = new AddStandardPointsCommandPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            StandardType = standardType,
            IsOverride = isOverride,
            IsAdmin = isAdminOrSecretary,
            Comment = comment,
        };
        var res = await addStandardPointsCommand.ExecuteAsync(addPointsForStandardPayload);

        return res.Match(
            _ => true,
            exception =>
            {
                logger.LogWarning(exception, "Something bad happened");
                throw exception;
            }
        );
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(VisitExpiredException))]
    [Error(typeof(VisitAlreadyExistsException))]
    [Error(typeof(ActionFromFutureException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> IncreaseStudentVisits(
        string studentGuid,
        DateOnly date,
        [Service] IncreaseStudentVisitsCommand increaseStudentVisitsCommand,
        [Service] PermissionValidator permissionValidator,
        [Service] ApplicationContext applicationContext,
        [Service] ILogger<StudentMutationExtensions> logger,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var increaseStudentVisitsPayload = new IncreaseStudentVisitsCommandPayload
        {
            Date = date,
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            IsAdmin = isAdminOrSecretary,
        };

        var res = await increaseStudentVisitsCommand.ExecuteAsync(increaseStudentVisitsPayload);

        return res.Match(
            _ => true,
            exception =>
            {
                logger.LogWarning(exception, "Something bad happened");
                throw exception;
            }
        );
    }

    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPointsException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<ArchivedStudentEntity> ArchiveStudent(
        [Service] ArchiveStudentCommand archiveStudentCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var archiveStudentPayload = new ArchiveStudentCommandPayload { StudentGuid = studentGuid };

        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(archivedStudent => archivedStudent, exception => throw exception);
    }

    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> UpdateStudentsInfo(
        [Service] SyncStudentsCommand syncStudentCommand,
        [Service] ILogger<SyncStudentsCommand> logger,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        // We run this command in the background because it takes
        // to much time so client closes connection before command ends
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        {
            try
            {
                logger.LogInformation(
                    "Teacher: {teacherGuid} started {commandName}",
                    callerGuid,
                    nameof(SyncStudentsCommand)
                );

                await syncStudentCommand.ExecuteAsync(EmptyPayload.Empty);

                logger.LogInformation(
                    "{commandName} has successfully finished",
                    nameof(SyncStudentsCommand)
                );
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Unhandled exception happened in {commandName}",
                    nameof(SyncStudentsCommand)
                );
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
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

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
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await deActivateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(VisitsStudentHistoryNotFoundException))]
    [Error(typeof(TeacherGuidMismatchException))]
    [Error(typeof(VisitOutdatedException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeleteStudentVisit(
        int historyId,
        [Service] DeleteStudentVisitCommand deleteStudentVisitCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStudentVisitCommand.ExecuteAsync(
            new DeleteStudentVisitCommandPayload
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(PointsStudentHistoryNotFoundException))]
    [Error(typeof(TeacherGuidMismatchException))]
    [Error(typeof(PointsOutdatedException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeletePoints(
        int historyId,
        [Service] DeletePointsCommand deletePointsCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsOk;

        var res = await deletePointsCommand.ExecuteAsync(
            new DeletePointsCommandPayload
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StandardsStudentHistoryNotFoundException))]
    [Error(typeof(TeacherGuidMismatchException))]
    [Error(typeof(PointsOutdatedException))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeleteStandardPoints(
        int historyId,
        [Service] DeleteStandardPointsCommand deleteStandardPointsCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStandardPointsCommand.ExecuteAsync(
            new DeleteStandardPointsCommandPayload()
            {
                HistoryId = historyId,
                IsAdmin = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(StudentNotFoundException))]
    [Error(typeof(CuratorGuidMismatch))]
    public async Task<Success> AddStudentHealthGroup(
        string studentGuid,
        HealthGroupType healthGroup,
        [Service] AddHealthGroupCommand addHealthGroupCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var res = await addHealthGroupCommand.ExecuteAsync(
            new AddHealthGroupPayload
            {
                HealthGroup = healthGroup,
                StudentGuid = studentGuid,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> ClearStudentsHealthGroup(
        [Service] ClearStudentsHealthGroupCommand clearStudentsHealthGroupCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await clearStudentsHealthGroupCommand.ExecuteAsync(EmptyPayload.Empty);

        return res.Match(_ => true, exception => throw exception);
    }

    private static string GetCallerGuid(ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.Claims.First(c => c.Type == "IndividualGuid").Value;
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }

        return callerGuid;
    }
}
