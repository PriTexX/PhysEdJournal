namespace PhysEdJournal.Api.Endpoints.StaffEndpoint;

public sealed class GetStaffByFilterRequest
{
    public required string Filter { get; init; }

    private int _pageSize = 20;
    public required int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value;
    }
}

public sealed class GetStaffByFilterResponse
{
    public required IEnumerable<EmployeeResponse> Employees { get; init; }
}

public sealed class EmployeeResponse
{
    public required string FullName { get; init; }
    public required string Guid { get; init; }
}
