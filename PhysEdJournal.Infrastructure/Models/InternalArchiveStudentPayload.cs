using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Infrastructure.Models;

internal sealed class InternalArchiveStudentPayload
{
    public required string StudentGuid { get; init; }
    public required int Visits { get; init; }
    public required string FullName { get; init; }
    public required string GroupNumber { get; init; }
    public required string CurrentSemesterName { get; init; }
    public required string ActiveSemesterName { get; init; }
    public required double TotalPoints { get; init; }

    public required bool HasDebt { get; init; }

    public required ICollection<VisitStudentHistoryEntity>? VisitStudentHistory { get; init; }

    public required ICollection<PointsStudentHistoryEntity>? PointsStudentHistory { get; init; }

    public required ICollection<StandardsStudentHistoryEntity>? StandardsStudentHistory { get; init; }
}
