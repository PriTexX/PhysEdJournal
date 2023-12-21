using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public static class AddPointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(FitnessAlreadyExistsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "fitness-duplicate",
                        Title = "Record for fitness already exists",
                        Detail = "Only one record of points for external fitness per semester",
                    }
            },
            {
                nameof(PointsExceededLimit),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "points-out-of-limit",
                        Title = "Points limit is exceeded",
                        Detail = "You can't give this much points for such an activity",
                    }
            },
            {
                nameof(VisitExpiredException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-expired",
                        Title = "This visit is too old",
                        Detail = "You can't set visit for a day that was so long ago.",
                    }
            },
            {
                nameof(VisitAlreadyExistsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-exists",
                        Title = "This visit already exists",
                        Detail = "You cannot set more than one visit per day",
                    }
            },
        };
}
