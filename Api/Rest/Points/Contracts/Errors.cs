using Api.Rest.Common;
using Core.Commands;
using Core.Config;

namespace Api.Rest.Points.Contracts;

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
                nameof(GTOExistsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "gto-duplicate",
                    Detail = "Баллы за ГТО можно поставить только раз в семестр",
                }
            },
            {
                nameof(StandardExistsError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "standard-duplicate",
                    Detail = "Данный норматив уже проставлен",
                }
            },
            {
                nameof(PointsOutOfLimitError),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "points-out-of-limit",
                    Detail = "Превышен максимальный балл за данную активность",
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
        };
}
