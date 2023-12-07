using System.Security.Claims;

namespace PhysEdJournal.Api.Rest.Requests;

public sealed class AssignCuratorToGroupRequest
{
    public required string GroupName { get; init; }

    public required string TeacherGuid { get; init; }

    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
}
