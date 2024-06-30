using Core.Commands;

namespace Api.Rest.Common;

public static class ErrorHandler
{
    public static ErrorResponse HandleErrorResult(Exception error)
    {
        var errorName = error.GetType().Name;
        return Errors.TryGetValue(errorName, out var errorProblemDetailsBuilder)
            ? errorProblemDetailsBuilder(error)
            : DefaultErrorResponse;
    }

    public static void AddErrors(Dictionary<string, Func<Exception, ErrorResponse>> errors)
    {
        foreach (var kvp in errors)
        {
            Errors[kvp.Key] = kvp.Value;
        }
    }

    private static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPermissionsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "not-enough-permissions",
                    Detail = "Недостаточно прав",
                }
            },
            {
                nameof(TeacherNotFoundError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "no-teacher",
                    Detail = "Преподаватель отсутствует в системе",
                }
            },
            {
                nameof(StudentNotFoundError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "no-student",
                    Detail = "Студент отсутствует в журнале",
                }
            },
            {
                nameof(ActionFromFutureError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "action-from-future",
                    Detail = "Нельзя ставить баллы за будущую дату",
                }
            },
            {
                nameof(DateExpiredError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "add-history-expired",
                    Detail = "Возможность выставлять баллы за данную дату закрыта",
                }
            },
            {
                nameof(HistoryDeleteExpiredError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "delete-history-expired",
                    Detail = "Возможность удалять баллы за данную дату закрыта",
                }
            },
            {
                nameof(NonWorkingDayError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "non-working-day",
                    Detail = "Нельзя выставлять баллы в нерабочие дни",
                }
            },
            {
                nameof(TeacherMismatchError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "wrong-teacher",
                    Detail = "Нельзя менять чужие данные",
                }
            },
            {
                nameof(OrderByQueryStructureError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "order-by-query",
                    Detail =
                        "Запрос на сортировку это список параметров через запятую. Параметр на сортировку состоит из единого слова, а так же может иметь модификатор asc или desc.",
                }
            },
        };

    private static readonly ErrorResponse DefaultErrorResponse =
        new()
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Type = "unknown-error",
            Detail = "Неизвестная ошибка",
        };
}
