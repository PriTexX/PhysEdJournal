using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public class AddPointsToStudentRequest
{
    public required string StudentGuid { get; init; }
    public required int PointsAmount { get; init; }
    public required DateOnly Date { get; init; }
    public required WorkType WorkType { get; init; }
    public string? Comment { get; init; } = null;
}
