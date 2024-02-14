using PhysEdJournal.Api.Api.ResponseType;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;

namespace PhysEdJournal.Api.Api.Semester.Contracts;

public static class SemesterErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(SemesterNameValidationException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "semester-name-wrong",
                        Title = "Ошибка при валидации названия семестра",
                        Detail =
                            "Название семестра должно соответствовать паттерну: 2022-2023/весна",
                    }
            },
        };
}
