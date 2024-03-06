using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest.Common;

public static class ErrorHandler
{
    public static ProblemDetailsResponse HandleErrorResult(Exception error)
    {
        var errorName = error.GetType().Name;
        return Errors.TryGetValue(errorName, out var errorProblemDetailsBuilder)
            ? errorProblemDetailsBuilder(error)
            : DefaultProblemDetailsResponse;
    }

    public static void AddErrors(Dictionary<string, Func<Exception, ProblemDetailsResponse>> errors)
    {
        foreach (var kvp in errors)
        {
            Errors[kvp.Key] = kvp.Value;
        }
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
                        Title = "Не достаточно прав",
                        Detail = "У пользователя должно быть больше прав для этого действия",
                    }
            },
            {
                nameof(TeacherNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "no-teacher",
                        Title = "Учитель не найден",
                        Detail = "Учитель не был обнаружен в базе данных",
                    }
            },
            {
                nameof(StudentNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "no-student",
                        Title = "Студент не найден",
                        Detail = "Студент не был найден в системе",
                    }
            },
            {
                nameof(ActionFromFutureException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "action-from-future",
                        Title = "Действие было совершено в будущем",
                        Detail = "Система не может обрабатывать действия, которые еще не произошли",
                    }
            },
            {
                nameof(DateExpiredException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "date-expired",
                        Title = "Действие было совершено слишком поздно",
                        Detail = "Действие должно быть исполнено перед дедлайном",
                    }
            },
            {
                nameof(NegativePointAmount),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "negative-points",
                        Title = "Кол-во баллов меньше или равно нулю",
                        Detail = "Нельзя выставить отрицательное кол-во баллов",
                    }
            },
            {
                nameof(NonWorkingDayException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "non-working-day",
                        Title = "Попытка выполнить действие в нерабочий день",
                        Detail = "Нерабочие дни: Воскресенье, Понедельник",
                    }
            },
            {
                nameof(ConcurrencyError),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "concurrency",
                        Title = "Попытка выполнить действие дважды",
                        Detail = "Нельзя выполнять одно и то же действие дважды",
                    }
            },
            {
                nameof(TeacherGuidMismatchException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "wrong-teacher",
                        Title = "Действие должно быть выполнено другим учителем",
                        Detail = "Учитель должен изменять только свои данные.",
                    }
            },
        };

    private static readonly ProblemDetailsResponse DefaultProblemDetailsResponse =
        new()
        {
            Status = 500,
            Type = "unknown-error",
            Title = "Неизвестная ошибка",
            Detail = "Неизвестная ошибка произошла во время запроса",
        };
}
