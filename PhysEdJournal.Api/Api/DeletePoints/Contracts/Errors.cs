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
            {
                nameof(ArchivedVisitDeletionException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "archived-visit-deletion",
                        Title = "Attempt to delete archived visit",
                        Detail = "You cannot delete archived visits",
                    }
            },
            {
                nameof(VisitOutdatedException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-outdated",
                        Title = "Attempt to delete outdated visit",
                        Detail = "You cannot delete visits that were set too long ago",
                    }
            },
        };
}
