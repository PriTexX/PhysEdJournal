using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.GroupExceptions;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

public static class GroupErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NullVisitValueException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "no-visit-value",
                        Title = "Кол-во баллов за посещение не было передано",
                        Detail =
                            "Невозможно передать отрецательное значение для кол-ва баллов за посещение.",
                    }
            },
        };
}
