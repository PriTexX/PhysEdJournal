using DB.Tables;

namespace Api.Rest.Student.Contracts;

public sealed class GetStudentsRequest
{
    public string? FullName { get; init; }
    public string? GroupNumber { get; init; }
    public int? Course { get; init; }
}

public sealed class StudentResponse
{
    public required string StudentGuid { get; set; }
    public required string FullName { get; set; }
    public required string GroupNumber { get; set; }
    public required int Course { get; set; }
    public required int Visits { get; set; }
    public required double TotalPoints { get; set; }
    public required int StandardPoints { get; set; }
    public required int LMSPoints { get; set; }
    public required bool HasDebt { get; set; }
    public required HealthGroupType HealthGroup { get; set; }
    public required SpecializationType Specialization { get; set; }
}

public sealed class GetStudentsResponse
{
    public required List<StudentResponse> Students { get; set; }
    public required int TotalCount { get; set; }
}
