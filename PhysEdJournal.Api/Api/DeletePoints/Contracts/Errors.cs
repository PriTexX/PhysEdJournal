using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.Api.DeletePoints.Contracts;

public static class DeletePointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(VisitsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "visit-not-found",
                        Title = "Visit not found in the system",
                        Detail = "Visit record was not found in the database",
                    }
            },
        };
}
