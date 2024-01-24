using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
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
            {
                nameof(PointsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "points-history-not-found",
                        Title = "Points student history not found",
                        Detail = "Points student history record was not found in the database",
                    }
            },
            {
                nameof(StandardsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "standards-history-not-found",
                        Title = "Standards student history not found",
                        Detail = "Standards student history record was not found in the database",
                    }
            },
        };
}
