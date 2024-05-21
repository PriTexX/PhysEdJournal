using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.GroupExceptions;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

public static class GroupErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(WrongVisitValueException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "wrong-visit-value",
                    Detail =
                        "Количество баллов за посещение должно быть одним из 2; 2.5; 3; 3.5; 4",
                }
            },
            {
                nameof(GroupNotFoundException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Type = "group-not-found",
                    Detail = "Группа не найдена",
                }
            },
        };
}
