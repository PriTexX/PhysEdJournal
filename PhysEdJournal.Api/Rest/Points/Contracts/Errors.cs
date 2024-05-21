using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;

namespace PhysEdJournal.Api.Rest.Points.Contracts;

public static class PointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(FitnessAlreadyExistsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "fitness-duplicate",
                    Detail = "Баллы за фитнес можно поставить только раз в семестр",
                }
            },
            {
                nameof(PointsExceededLimit),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "points-out-of-limit",
                    Detail = $"Максимальный балл за данную активность {Constants.MaxPointsAmount}",
                }
            },
            {
                nameof(VisitExpiredException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "visit-expired",
                    Detail = "Нельзя поставить посещение за дату старше 7 дней",
                }
            },
            {
                nameof(VisitAlreadyExistsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "visit-exists",
                    Detail = "Нельзя поставить больше одного посещения в день",
                }
            },
            {
                nameof(VisitsStudentHistoryNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "visit-not-found",
                    Detail = "Не удалось найти посещение в системе",
                }
            },
            {
                nameof(VisitOutdatedException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "visit-outdated",
                    Detail = "Нельзя удалить посещения старше 7 дней",
                }
            },
            {
                nameof(PointsStudentHistoryNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "points-history-not-found",
                    Detail = "Не удалось найти историю баллов в системе",
                }
            },
            {
                nameof(StandardsStudentHistoryNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "standards-history-not-found",
                    Detail = "Не удалось найти норматив в системе",
                }
            },
            {
                nameof(StandardAlreadyExistsException),
                _ => new ErrorResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "standard-duplicate",
                    Detail = "Нельзя сдать один и тот же норматив дважды",
                }
            },
        };
}
