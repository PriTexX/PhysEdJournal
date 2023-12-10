using PhysEdJournal.Core.Exceptions.GroupExceptions;

namespace PhysEdJournal.Api.Api.Group.Contracts;

public static class GroupErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NullVisitValueException),
                _ => new ProblemDetailsResponse
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "no-visit-value",
                    Title = "Visit value was not provided",
                    Detail = "Visit value cannot be negative.",
                }
            },
        };
}