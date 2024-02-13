using PhysEdJournal.Api.Api._Response;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.Api.DeletePoints.Contracts;

public static class DeletePointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(VisitsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "visit-not-found",
                        Title = "Запись о посещении не существует",
                        Detail = "Запись о посещении не была найдена в базе данных",
                    }
            },
            {
                nameof(VisitOutdatedException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-outdated",
                        Title = "Попытка удалить посещение, которое было очень давно",
                        Detail = "Нельзя удалить посещения, которые были так давно",
                    }
            },
            {
                nameof(PointsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "points-history-not-found",
                        Title = "История баллов не существует",
                        Detail = "Не удалось найти историю баллов в базе данных",
                    }
            },
            {
                nameof(StandardsStudentHistoryNotFoundException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "standards-history-not-found",
                        Title = "Не удается найти историю нормативов",
                        Detail = "Запись о сдаче нормативов не существует в базе данных",
                    }
            },
        };
}
