using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;

namespace PhysEdJournal.Api.Rest.Semester.Contracts;

public static class SemesterErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(SemesterNameValidationException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "wrong-semester-name",
                    Detail = "Название семестра должно соответствовать шаблону: 2022-2023/весна",
                }
            },
        };
}
