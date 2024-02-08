using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Api.Competition.Contracts;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Api.Competition;

public static class CompetitionController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(CompetitionErrors.Errors);

        router.MapPost("/CreateCompetition", CreateCompetition);
        router.MapDelete("/DeleteCompetition", DeleteCompetition);
    }

    public static async Task<IResult> CreateCompetition(
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

        return result.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }

    public static async Task<IResult> DeleteCompetition(
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

        return result.Match(_ => Results.Ok(), ErrorHandler.HandleErrorResult);
    }
}
