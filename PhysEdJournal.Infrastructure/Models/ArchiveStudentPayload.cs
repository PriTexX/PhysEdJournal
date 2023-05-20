namespace PhysEdJournal.Infrastructure.Models;

public sealed class ArchiveStudentPayload
{
    public required string StudentGuid { get; init; }
    public required int Visits { get; init; }
    public required string FullName { get; init; }
    public required string GroupNumber { get; init; }
    public required string CurrentSemesterName { get; init; }
    public required string ActiveSemesterName { get; init; }
    public required double TotalPoints { get; init; }
}