namespace Api.Rest.Teacher.Contracts;

public sealed class GetTeacherResponse
{
    public required string FullName { get; set; }
    public required List<string> Groups { get; set; }
    public required List<string> Permissions { get; set; }
}
