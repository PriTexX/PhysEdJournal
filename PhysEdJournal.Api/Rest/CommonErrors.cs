using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest;

public static class CommonErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPermissionsException),
                err =>
                {
                    var message = ((NotEnoughPermissionsException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "forbidden",
                        Title = "Not enough permissions",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(TeacherNotFoundException),
                err =>
                {
                    var message = ((TeacherNotFoundException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "not-found",
                        Title = "Teacher not found",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(StudentNotFoundException),
                err =>
                {
                    var message = ((StudentNotFoundException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "not-found",
                        Title = "Student not found",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(ActionFromFutureException),
                err =>
                {
                    var message = ((ActionFromFutureException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "bad-request",
                        Title = "Action was committed in the future",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(DateExpiredException),
                err =>
                {
                    var message = ((DateExpiredException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "bad-request",
                        Title = "Action was performed after the deadline",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(NegativePointAmount),
                err =>
                {
                    var message = ((NegativePointAmount)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "bad-request",
                        Title = "Attempt to perform action after the deadline",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(NonWorkingDayException),
                err => 
                {
                    var message = ((NonWorkingDayException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "bad-request",
                        Title = "Attempt to perform action on the non working day",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(ConcurrencyError),
                err =>
                {
                    var message = ((ConcurrencyError)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "conflict",
                        Title = "Attempt to perform one action twice",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(TeacherGuidMismatchException),
                err =>
                {
                    var message = ((TeacherGuidMismatchException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "conflict",
                        Title = "This action should be done by another teacher",
                        Detail = $"{message}"
                    };
                }
            },
            {
                nameof(ArchivedPointsDeletionException),
                err =>
                {
                    var message = ((ArchivedPointsDeletionException)err).Message;
                    return new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "bad-request",
                        Title = "Attempt to delete archived points",
                        Detail = $"{message}"
                    };
                }
            }
        };
}