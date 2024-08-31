using DB.Tables;

namespace Api.Rest.Student.Contracts;

public sealed class VisitsHistoryResponse
{
    public required int Id { get; set; }
    public required DateOnly Date { get; set; }
    public required string TeacherGuid { get; set; }
    public required string TeacherFullName { get; set; }
}

public sealed class PointsHistoryResponse
{
    public required int Id { get; set; }
    public required DateOnly Date { get; set; }
    public required int Points { get; set; }
    public required WorkType Type { get; set; }
    public required string? Comment { get; set; }
    public required string TeacherGuid { get; set; }
    public required string TeacherFullName { get; set; }
}

public sealed class StandardsHistoryResponse
{
    public required int Id { get; set; }
    public required DateOnly Date { get; set; }
    public required int Points { get; set; }
    public required StandardType Type { get; set; }
    public required string? Comment { get; set; }
    public required string TeacherGuid { get; set; }
    public required string TeacherFullName { get; set; }
}

public sealed class Curator
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
}

public sealed class GetStudentResponse
{
    public required string StudentGuid { get; set; }
    public required string FullName { get; set; }
    public required string GroupNumber { get; set; }
    public required bool HasDebt { get; set; }
    public required bool HadDebtInSemester { get; set; }
    public required double TotalPoints { get; set; }
    public required int LMSPoints { get; set; }
    public required int Course { get; set; }
    public Curator? Curator { get; set; }
    public required HealthGroupType HealthGroup { get; set; }
    public required List<PointsHistoryResponse> PointsHistory { get; set; }
    public required List<VisitsHistoryResponse> VisitsHistory { get; set; }
    public required List<StandardsHistoryResponse> StandardsHistory { get; set; }
}
