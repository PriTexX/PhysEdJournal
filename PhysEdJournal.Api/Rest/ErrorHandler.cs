using PhysEdJournal.Api.Rest;

namespace PhysEdJournal.Api.Controllers;

public static class ErrorHandler
{
    public static ProblemDetailsResult HandleErrorResult(
        Dictionary<string, Func<Exception, ProblemDetailsResponse>> dict,
        Exception error
    )
    {
        var errorName = error.GetType().Name;
        return dict.TryGetValue(errorName, out var errorProblemDetailsBuilder)
            ? new ProblemDetailsResult(errorProblemDetailsBuilder(error))
            : CommonErrors.Errors.TryGetValue(errorName, out var commonErrorProblemDetailsBuilder)
                ? new ProblemDetailsResult(commonErrorProblemDetailsBuilder(error))
                : new ProblemDetailsResult(DefaultProblemDetailsResponse);
    }

    private static readonly ProblemDetailsResponse DefaultProblemDetailsResponse =
        new()
        {
            Status = 500,
            Type = "unknown-error",
            Title = "Unknown error",
            Detail = "Unknown error happened during request"
        };
}