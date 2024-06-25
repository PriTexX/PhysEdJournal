using System.Security.Claims;
using Core.Commands;
using DB.Tables;
using GraphQL.Api.ScalarTypes;

namespace GraphQL.Api.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class CompetitionMutationExtensions
{
    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    public async Task<Success> CreateCompetition(
        string competitionName,
        [Service] CreateCompetitionCommand createCompetitionCommand,
        [Service] GraphQLPermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var result = await createCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundError))]
    [Error(typeof(GraphQLNotEnoughPermissionsError))]
    public async Task<Success> DeleteCompetition(
        string competitionName,
        [Service] DeleteCompetitionCommand deleteCompetitionCommand,
        [Service] GraphQLPermissionValidator permissionValidator,
        ClaimsPrincipal claimsPrincipal
    )
    {
        var callerGuid = GetCallerGuid(claimsPrincipal);

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var result = await deleteCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(_ => true, exception => throw exception);
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
