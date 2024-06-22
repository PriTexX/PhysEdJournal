using Core.Cfg;
using Core.Commands;
using PhysEdJournal.Api.Rest.Common;

namespace PhysEdJournal.Api.Rest.Points.Contracts;

public static class PointsErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(FitnessExistsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "fitness-duplicate",
                    Detail = "Баллы за фитнес можно поставить только раз в семестр",
                }
            },
            {
                nameof(PointsOutOfLimitError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "points-out-of-limit",
                    Detail = $"Максимальный балл за данную активность {Config.MaxPointsAmount}",
                }
            },
            {
                nameof(VisitExistsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "visit-exists",
                    Detail = "Нельзя поставить больше одного посещения в день",
                }
            },
            {
                nameof(HistoryNotFoundError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "points-history-not-found",
                    Detail = "Не удалось найти историю баллов в системе",
                }
            },
            {
                nameof(StandardExistsError),
                _ => new ErrorResponse()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "standard-duplicate",
                    Detail = "Нельзя сдать один и тот же норматив дважды",
                }
            },
        };
}
