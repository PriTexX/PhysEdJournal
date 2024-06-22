using DB.Tables;

namespace PhysEdJournal.Api.Rest.Student.Contracts.Responses;

public sealed class GetStudentResponse
{
    public required string StudentGuid { get; set; }

    public required string FullName { get; set; }

    public required string GroupNumber { get; set; }

    public bool HasDebtFromPreviousSemester { get; set; }

    public bool HadDebtInSemester { get; set; }

    public double ArchivedVisitValue { get; set; }

    public int AdditionalPoints { get; set; }

    public int PointsForStandards { get; set; }

    public bool IsActive { get; set; } = true;

    public int Visits { get; set; }

    public int Course { get; set; }

    public required string CurrentSemesterName { get; set; }

    public HealthGroupType HealthGroup { get; set; }

    public string? Department { get; set; }

    public uint Version { get; set; }

    public IList<PointsStudentHistoryEntity>? PointsStudentHistory { get; set; }

    public IList<VisitStudentHistoryEntity>? VisitsStudentHistory { get; set; }

    public IList<StandardsStudentHistoryEntity>? StandardsStudentHistory { get; set; }
}
