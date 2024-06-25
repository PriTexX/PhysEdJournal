namespace Api.Rest.Student.Contracts;

public sealed class StudentFilterParameters
{
    public string? FullName { get; init; }

    public string? GroupNumber { get; init; }

    public bool IsActive { get; init; } = true;

    public int? Course { get; init; }
}
