namespace PhysEdJournal.Api.Endpoints.StaffEndpoint;

public sealed class Employee
{
    public required string Guid { get; init; }
    public required string FullName { get; init; }
}

public sealed class Employees
{
    public required List<Employee> Items { get; init; }
}

public sealed class PagedStaffResponse
{
    public required Employees Employees { get; init; }
}