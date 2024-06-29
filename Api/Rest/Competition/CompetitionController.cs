using Api.Rest.Common;
using Api.Rest.Common.Filters;
using Api.Rest.Competition;
using Core.Commands;
using DB.Tables;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest;

public static class CompetitionController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(CompetitionErrors.Errors);

        var competitionRouter = router.MapGroup("competition");

        competitionRouter
            .MapPost("/", CreateCompetition)
            .AddPermissionsValidation(
                TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
            )
            .RequireAuthorization();

        competitionRouter
            .MapDelete("/", DeleteCompetition)
            .AddPermissionsValidation(
                TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
            )
            .RequireAuthorization();
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
