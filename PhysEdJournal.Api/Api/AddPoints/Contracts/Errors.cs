using PhysEdJournal.Api.Api._Response;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public static class AddPointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(FitnessAlreadyExistsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "fitness-duplicate",
                        Title = "Такая запись за фитнес уже существует",
                        Detail = "Баллы за фитнес можно получить только один раз в семестр",
                    }
            },
            {
                nameof(PointsExceededLimit),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "points-out-of-limit",
                        Title = "Лимит баллов превышен",
                        Detail = "Вы не можете выставить столько баллов за эту активность",
                    }
            },
            {
                nameof(VisitExpiredException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-expired",
                        Title = "Эта запись посещений слишком старая",
                        Detail = "Вы не можете удалить посещение, которые было так давно",
                    }
            },
            {
                nameof(VisitAlreadyExistsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "visit-exists",
                        Title = "Это посещение уже существует",
                        Detail = "Нельзя поставить больше одного посещения в день",
                    }
            },
        };
}
