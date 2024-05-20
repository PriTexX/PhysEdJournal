using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;

namespace PhysEdJournal.Api.Rest.Student.Contracts;

public static class StudentErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPointsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "not-enough-points",
                    Detail =
                        "Для перевода в следующий семестр студенту необходимо набрать более 50 баллов",
                }
            },
        };
}
