using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TeacherMutationExtensions
{
    [Error(typeof(TeacherAlreadyExistsException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<TeacherEntity> CreateTeacherAsync(
        string teacherGuid,
        string fullName,
        [Service] CreateTeacherCommand createTeacherCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var createTeacherPayload = new CreateTeacherCommandPayload
        {
            TeacherGuid = teacherGuid,
            FullName = fullName,
            Permissions = TeacherPermissions.DefaultAccess
        };

        var result = await createTeacherCommand.ExecuteAsync(createTeacherPayload);

        return result.Match(
            _ =>
                new TeacherEntity
                {
                    FullName = fullName,
                    TeacherGuid = teacherGuid,
                    Permissions = TeacherPermissions.DefaultAccess
                },
            exception => throw exception
        );
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(CannotGrantSuperUserPermissionsException))]
    public async Task<Success> GivePermissionsToTeacherAsync(
        string teacherGuid,
        IEnumerable<TeacherPermissions> permissions,
        [Service] GivePermissionsCommand givePermissionsCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        var teacherPermissions = permissions.Aggregate((prev, next) => prev | next);

        if (teacherPermissions.HasFlag(TeacherPermissions.AdminAccess))
        {
            await permissionValidator.ValidateTeacherPermissionsAndThrow(
                callerGuid,
                FOR_ONLY_SUPERUSER_USE_PERMISSIONS
            );
        }
        else
        {
            await permissionValidator.ValidateTeacherPermissionsAndThrow(
                callerGuid,
                FOR_ONLY_ADMIN_USE_PERMISSIONS
            );
        }

        var givePermissionsPayload = new GivePermissionsCommandPayload
        {
            TeacherGuid = teacherGuid,
            TeacherPermissions = teacherPermissions
        };

        var result = await givePermissionsCommand.ExecuteAsync(givePermissionsPayload);

        return result.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    public async Task<Success> CreateCompetition(
        string competitionName,
        [Service] CreateCompetitionCommand createCompetitionCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var result = await createCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(CompetitionNotFoundException))]
    public async Task<Success> DeleteCompetition(
        string competitionName,
        [Service] DeleteCompetitionCommand deleteCompetitionCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        ThrowIfCallerGuidIsNull(callerGuid);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var result = await deleteCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(_ => true, exception => throw exception);
    }

    private static void ThrowIfCallerGuidIsNull(string? callerGuid)
    {
        if (callerGuid is null)
        {
            throw new Exception("IndividualGuid cannot be empty. Wrong token was passed");
        }
    }
}
