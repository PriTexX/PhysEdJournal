using PhysEdJournal.Core.Exceptions.DateExceptions;
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
        };
}