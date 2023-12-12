namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public static class AddPointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NullVisitValueException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "no-visit-value",
                        Title = "Visit value was not provided",
                        Detail = "Visit value cannot be negative.",
                    }
            },
        };
}
