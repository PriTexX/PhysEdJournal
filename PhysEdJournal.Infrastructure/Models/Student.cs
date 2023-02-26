namespace PhysEdJournal.Infrastructure.Models;

public sealed class Student
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
    public required string Group { get; set; }
    public required int Course { get; set; }
    public required string Department { get; set; }
}

public sealed class Students
{
    public required bool HasNextPage { get; set; }
    public required List<Student> Items { get; set; }
}

public sealed class PagedGraphQLStudent
{
    public required Students Students { get; set; }
}