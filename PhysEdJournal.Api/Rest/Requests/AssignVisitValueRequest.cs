using System.Security.Claims;

namespace PhysEdJournal.Api.Rest.Requests;

public sealed class AssignVisitValueRequest
{
    public required string GroupName { get; init; }

    public required double NewVisitValue { get; init; }

    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
}
