using System.Security.Claims;
using Core.Commands;
using DB;
using DB.Tables;
using GraphQL.Api.ScalarTypes;
using Microsoft.Extensions.Logging;

namespace GraphQL.Api.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    [Error(typeof(StudentNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(ActionFromFutureError))]
    [Error(typeof(FitnessExistsError))]
    [Error(typeof(DateExpiredError))]
    [Error(typeof(PointsOutOfLimitError))]
    [Error(typeof(NonWorkingDayError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> AddPointsToStudent(
        [Service] AddPointsCommand addPointsCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
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
                await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    TeacherPermissions.SecretaryAccess
                );
                break;
            }

            case WorkType.OnlineWork:
            {
                await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    TeacherPermissions.OnlineCourseAccess
                );
                break;
            }
        }

        var validateTeacherPermissionsResult =
            await graphQLPermissionValidator.ValidateTeacherPermissions(
                callerGuid,
                TeacherPermissions.SecretaryAccess
            );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsPayload = new AddPointsPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            IsAdminOrSecretary = isAdminOrSecretary,
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

    [Error(typeof(StudentNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(StandardExistsError))]
    [Error(typeof(ActionFromFutureError))]
    [Error(typeof(NotEnoughPointsForStandardsError))]
    [Error(typeof(DateExpiredError))]
    [Error(typeof(NonWorkingDayError))]
    [Error(typeof(PointsOutOfLimitError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> AddPointsForStandardToStudent(
        string studentGuid,
        int pointsAmount,
        DateOnly date,
        StandardType standardType,
        bool isOverride,
        [Service] AddStandardCommand addStandardPointsCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        [Service] ILogger<StudentMutationExtensions> logger,
        ClaimsPrincipal claimsPrincipal,
        string? comment = null
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult =
            await graphQLPermissionValidator.ValidateTeacherPermissions(
                callerGuid,
                TeacherPermissions.SecretaryAccess
            );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsForStandardPayload = new AddStandardPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            Points = pointsAmount,
            Date = date,
            StandardType = standardType,
            IsAdminOrSecretary = isAdminOrSecretary,
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

    [Error(typeof(StudentNotFoundError))]
    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    [Error(typeof(DateExpiredError))]
    [Error(typeof(VisitExistsError))]
    [Error(typeof(ActionFromFutureError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> IncreaseStudentVisits(
        string studentGuid,
        DateOnly date,
        [Service] AddVisitCommand increaseStudentVisitsCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        [Service] ApplicationContext applicationContext,
        [Service] ILogger<StudentMutationExtensions> logger,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult =
            await graphQLPermissionValidator.ValidateTeacherPermissions(
                callerGuid,
                TeacherPermissions.SecretaryAccess
            );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var increaseStudentVisitsPayload = new AddVisitPayload
        {
            Date = date,
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            IsAdminOrSecretary = isAdminOrSecretary,
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

    [Error(typeof(StudentNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(NotEnoughPointsError))]
    [Error(typeof(NotCuratorError))]
    [Error(typeof(SameSemesterError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<ArchivedStudentEntity> ArchiveStudent(
        [Service] ArchiveStudentCommand archiveStudentCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        ClaimsPrincipal claimsPrincipal,
        string studentGuid
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validationRes = await graphQLPermissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var archiveStudentPayload = new ArchiveStudentPayload
        {
            StudentGuid = studentGuid,
            TeacherGuid = callerGuid,
            IsAdmin = validationRes.IsOk,
        };

        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(archivedStudent => archivedStudent, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(HistoryDeleteExpiredError))]
    [Error(typeof(TeacherMismatchError))]
    [Error(typeof(HistoryNotFoundError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeleteStudentVisit(
        int historyId,
        [Service] DeleteVisitCommand deleteStudentVisitCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await graphQLPermissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStudentVisitCommand.ExecuteAsync(
            new DeleteVisitPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(HistoryNotFoundError))]
    [Error(typeof(TeacherMismatchError))]
    [Error(typeof(HistoryDeleteExpiredError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeletePoints(
        int historyId,
        [Service] DeletePointsCommand deletePointsCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await graphQLPermissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deletePointsCommand.ExecuteAsync(
            new DeletePointsPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(HistoryNotFoundError))]
    [Error(typeof(TeacherMismatchError))]
    [Error(typeof(HistoryDeleteExpiredError))]
    [Error(typeof(ConcurrencyError))]
    public async Task<Success> DeleteStandardPoints(
        int historyId,
        [Service] DeleteStandardCommand deleteStandardPointsCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        var validationResult = await graphQLPermissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStandardPointsCommand.ExecuteAsync(
            new DeleteStandardPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(StudentNotFoundError))]
    [Error(typeof(CuratorMismatchError))]
    public async Task<Success> AddStudentHealthGroup(
        string studentGuid,
        HealthGroupType healthGroup,
        [Service] AddHealthGroupCommand addHealthGroupCommand,
        [Service] GraphQLPermissionValidator graphQLPermissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await graphQLPermissionValidator.ValidateTeacherPermissionsAndThrow(
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
