using Api.Rest.Common;
using Api.Rest.Common.Filters;
using Api.Rest.Competition.Contracts;
using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Rest;

public static class CompetitionController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(CompetitionErrors.Errors);

        var competitionRouter = router.MapGroup("competition");

        competitionRouter.MapGet("/", GetCompetitions);

        competitionRouter
            .MapPost("/", CreateCompetition)
            .AddPermissionsValidation(
                TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
            )
            .AddValidation(CreateCompetitionRequest.GetValidator())
            .RequireAuthorization();

        competitionRouter
            .MapDelete("/", DeleteCompetition)
            .AddPermissionsValidation(
                TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
            )
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateCompetition(
        [FromBody] CreateCompetitionRequest req,
        [FromServices] CreateCompetitionCommand createCompetitionCommand
    )
    {
        var result = await createCompetitionCommand.ExecuteAsync(req.CompetitionName);

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

    private static async Task<IResult> GetCompetitions([FromServices] ApplicationContext appCtx)
    {
        var competitions = await appCtx.Competitions.Select(c => c.CompetitionName).ToListAsync();

        return Results.Ok(new { competitions });
    }
}
