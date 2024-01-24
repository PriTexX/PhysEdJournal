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
                        Title = "Semester name validation failure",
                        Detail = "Semester name must be like: 2022-2023/весна",
                    }
            },
        };
}
