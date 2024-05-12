using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Common.Filters;
using PhysEdJournal.Api.Rest.Competition.Contracts;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using Response = PhysEdJournal.Api.Rest.Common.Response;

namespace PhysEdJournal.Api.Rest.Competition;

public static class CompetitionController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(CompetitionErrors.Errors);

        var competitionRouter = router.MapGroup("competition");

        competitionRouter
            .MapPost("/", CreateCompetition)
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);

        competitionRouter
            .MapDelete("/", DeleteCompetition)
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);
    }

    private static async Task<IResult> CreateCompetition(
        string competitionName,
        [FromServices] CreateCompetitionCommand createCompetitionCommand
    )
    {
        var result = await createCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeleteCompetition(
        string competitionName,
        [FromServices] DeleteCompetitionCommand deleteCompetitionCommand
    )
    {
        var result = await deleteCompetitionCommand.ExecuteAsync(competitionName);

        return result.Match(Response.Ok, Response.Error);
    }
}
