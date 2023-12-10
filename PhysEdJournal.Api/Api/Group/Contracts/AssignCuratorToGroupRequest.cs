namespace PhysEdJournal.Api.Api.Group.Contracts;

public sealed class AssignCuratorToGroupRequest
{
    public required string GroupName { get; init; }

    public required string TeacherGuid { get; init; }
}
