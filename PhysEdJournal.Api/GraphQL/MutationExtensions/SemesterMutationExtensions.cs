using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SemesterMutationExtensions
{
    [Error(typeof(SemesterNameValidationException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> StartNewSemester(
        string semesterName,
        [Service] StartNewSemesterCommand startNewSemesterCommand,
        [Service] PermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var res = await startNewSemesterCommand.ExecuteAsync(semesterName);

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
