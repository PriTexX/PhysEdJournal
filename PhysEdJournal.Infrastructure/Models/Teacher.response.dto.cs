namespace PhysEdJournal.Infrastructure.Models;

public sealed class Teacher
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
}

public sealed class Teachers
{
    public required bool HasNextPage { get; set; }
    public required List<Teacher> Items { get; set; }
}

public sealed class PagedGraphQLTeacher
{
    public required Teachers Employees { get; set; }
}
