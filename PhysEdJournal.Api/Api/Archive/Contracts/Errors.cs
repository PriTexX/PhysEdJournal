using PhysEdJournal.Core.Exceptions.StudentExceptions;

namespace PhysEdJournal.Api.Api.Archive.Contracts;

public static class ArchiveErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPointsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "not-enough-points",
                        Title = "Не хватает баллов для архивации",
                        Detail = "У студентов должно быть больше баллов для архивации",
                    }
            },
            {
                nameof(ArchivedStudentNotFound),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "archived-student-not-found",
                        Title = "Заархивированный студент не найден",
                        Detail = "Заархивированный студент не был найден в базе данных",
                    }
            },
        };
}
