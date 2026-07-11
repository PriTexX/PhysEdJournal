namespace Api.Rest.Groups.Contracts;

public sealed class GroupsResponse
{
    public required string GroupName { get; set; }
    public required string CuratorFullName { get; set; }
}
