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
            {
                nameof(NotCuratorError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "not-curator",
                    Detail = "Учитель, который архивирует, должен быть куратором группы",
                }
            },
            {
                nameof(SemesterMismatchError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "semester-mismatch",
                    Detail = "Нельзя перевести студента на семестр, на котором он уже обучается",
                }
            },
        };
}
