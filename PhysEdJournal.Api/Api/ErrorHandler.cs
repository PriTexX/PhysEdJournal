using PhysEdJournal.Api.Api;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Controllers;

public static class ErrorHandler
{
    public static IResult HandleErrorResult(Exception error)
    {
        var errorName = error.GetType().Name;
        return Errors.TryGetValue(errorName, out var errorProblemDetailsBuilder)
            ? ToResult(errorProblemDetailsBuilder(error))
            : ToResult(DefaultProblemDetailsResponse);
    }

    public static void AddErrors(Dictionary<string, Func<Exception, ProblemDetailsResponse>> errors)
    {
        foreach (var kvp in errors)
        {
            Errors[kvp.Key] = kvp.Value;
        }
    }

    private static IResult ToResult(ProblemDetailsResponse response)
    {
        return Results.Json(response, statusCode: response.Status);
    }

    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPermissionsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "not-enough-permissions",
                        Title = "Not enough permissions",
                        Detail = "User must have more permissions to perform this action",
                    }
            },
            {
                nameof(TeacherNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "no-teacher",
                        Title = "Teacher not found",
                        Detail = "Teacher was not found in the system",
                    }
            },
            {
                nameof(StudentNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "no-student",
                        Title = "Student not found",
                        Detail = "Student was not found in the system",
                    }
            },
            {
                nameof(ActionFromFutureException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "action-from-future",
                        Title = "Action was committed in the future",
                        Detail =
                            "The system does not operate with information that comes from the future",
                    }
            },
            {
                nameof(DateExpiredException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "date-expired",
                        Title = "Action was performed after the deadline",
                        Detail = "Action should be performed before its deadline",
                    }
            },
            {
                nameof(NegativePointAmount),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "negative-points",
                        Title = "Points amount is less than or equal to zero",
                        Detail = "Cannot grant negative or zero amount of points",
                    }
            },
            {
                nameof(NonWorkingDayException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "non-working-day",
                        Title = "Attempt to perform action on the non working day",
                        Detail = "Non Working days: Sunday, Monday",
                    }
            },
            {
                nameof(ConcurrencyError),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "concurrency",
                        Title = "Attempt to perform one action twice",
                        Detail = "Only one of each type of action at a time",
                    }
            },
            {
                nameof(TeacherGuidMismatchException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "wrong-teacher",
                        Title = "This action should be done by another teacher",
                        Detail =
                            "Teacher can manage the data that he has entered. He can't change other people's records.",
                    }
            },
            {
                nameof(ArchivedPointsDeletionException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "archive-points-deletion",
                        Title = "Attempt to delete archived points",
                        Detail = "You cannot delete points that are in the archive",
                    }
            },
        };

    private static readonly ProblemDetailsResponse DefaultProblemDetailsResponse =
        new()
        {
            Status = 500,
            Type = "unknown-error",
            Title = "Unknown error",
            Detail = "Unknown error happened during request",
        };
}
