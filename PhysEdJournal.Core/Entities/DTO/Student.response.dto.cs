namespace PhysEdJournal.Core.Entities.DTO;

public record struct StudentResponseDto
{
    public string FullName { get; init; }
    public string Guid { get; init; }
    public string Group { get; init; }
    public int Course { get; init; }
    public string Department { get; init; }
}