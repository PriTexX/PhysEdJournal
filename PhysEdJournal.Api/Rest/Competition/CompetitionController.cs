using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Competition.Contracts;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;
using Response = PhysEdJournal.Api.Rest.Common.Response;

namespace PhysEdJournal.Api.Rest.Competition;

public static class CompetitionController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(CompetitionErrors.Errors);

        router.MapPost("/create-competition", CreateCompetition);
        router.MapDelete("/delete-competition", DeleteCompetition);
    }

    private static async Task<IResult> CreateCompetition(
        string competitionName,
        [FromServices] CreateCompetitionCommand createCompetitionCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var result = await createCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeleteCompetition(
        string competitionName,
        [FromServices] DeleteCompetitionCommand deleteCompetitionCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var result = await deleteCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(Response.Ok, Response.Error);
    }
}
