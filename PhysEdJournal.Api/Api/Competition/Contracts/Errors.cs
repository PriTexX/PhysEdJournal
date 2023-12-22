using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Api.Competition.Contracts;

public static class CompetitionErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(CompetitionNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "competition-not-found",
                        Title = "Competition not found",
                        Detail = "Competition was not found in the database",
                    }
            },
        };
}
