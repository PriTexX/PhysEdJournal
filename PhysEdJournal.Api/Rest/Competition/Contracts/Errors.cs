using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest.Competition.Contracts;

public static class CompetitionErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(CompetitionNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "competition-not-found",
                    Detail = "Соревнование не найдено",
                }
            },
        };
}
