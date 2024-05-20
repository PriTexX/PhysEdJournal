namespace PhysEdJournal.Api.Rest.Student.Contracts.Responses;

public class GetStudentsResponse
{
    public required string StudentGuid { get; set; }

    public required string FullName { get; set; }

    public required string GroupNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public int Course { get; set; }
}
