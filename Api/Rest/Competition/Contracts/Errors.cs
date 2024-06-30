using Api.Rest.Common;
using Core.Commands;

namespace Api.Rest.Competition.Contracts;

public static class CompetitionErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(CompetitionNotFoundError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "competition-not-found",
                    Detail = "Соревнование не найдено",
                }
            },
        };
}
