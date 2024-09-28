using Api.Rest.Common;
using Core.Commands;

namespace Api.Rest.Student.Contracts;

public static class StudentErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPointsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "not-enough-points",
                    Detail =
                        "Для перевода в следующий семестр студенту необходимо набрать 50 баллов",
                }
            },
            {
                nameof(NotCuratorError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "not-curator",
                    Detail = "Для перевода группы в следующий семестр нужно быть куратором",
                }
            },
            {
                nameof(SameSemesterError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "same-semester",
                    Detail = "Студент уже переведен",
                }
            },
        };
}
