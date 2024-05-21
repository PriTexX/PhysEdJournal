using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest.Common;

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
                nameof(NotEnoughPermissionsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "not-enough-permissions",
                    Detail = "У пользователя должно быть больше прав для этого действия",
                }
            },
            {
                nameof(TeacherNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "no-teacher",
                    Detail = "Преподаватель отсутствует в системе",
                }
            },
            {
                nameof(StudentNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "no-student",
                    Detail = "Студент отсутствует в журнале",
                }
            },
            {
                nameof(ActionFromFutureException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "action-from-future",
                    Detail = "Укажите уже наступившую дату",
                }
            },
            {
                nameof(DateExpiredException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "date-expired",
                    Detail = "Срок давности указанной даты истек",
                }
            },
            {
                nameof(NegativePointAmount),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "negative-points",
                    Detail = "Количество баллов должно быть положительным",
                }
            },
            {
                nameof(NonWorkingDayException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "non-working-day",
                    Detail = "Нельзя выставлять баллы в нерабочие дни",
                }
            },
            {
                nameof(TeacherGuidMismatchException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "wrong-teacher",
                    Detail = "Нельзя менять чужие данные",
                }
            },
            {
                nameof(OrderByQueryStructureException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "order-by-query",
                    Detail =
                        "Запрос на сортировку это список параметров через запятую. Параметр на сортировку состоит из единого слова, а так же может иметь модификатор asc или desc.",
                }
            },
            {
                nameof(LoweringTheScoreException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "lowering-score",
                    Detail = "Нельзя занизить количество баллов студенту",
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
