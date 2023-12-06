using System.Security.Claims;

namespace PhysEdJournal.Api.Controllers.Requests;

public sealed class AssignVisitValueRequest
{
    public required string GroupName { get; init; }

    public required double NewVisitValue { get; init; }

    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
}
