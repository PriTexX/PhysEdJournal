namespace Api.Rest.Contracts;

public sealed class GroupsResponse
{
    public required string GroupName { get; set; }
    public required string CuratorFullName { get; set; }
}
