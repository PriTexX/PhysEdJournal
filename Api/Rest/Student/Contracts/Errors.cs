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
                nameof(SameSemesterError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "same-semester",
                    Detail = "Нельзя перевести студента на семестр, на котором он уже обучается",
                }
            },
        };
}
