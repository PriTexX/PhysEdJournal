using Core.Commands.SyncStudents;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api.StaffSearch;

public sealed class SearchStaffRequest
{
    public required string Filter { get; init; }
}

public sealed class SearchStaffResponse
{
    public required IEnumerable<EmployeeResponse> Employees { get; init; }
}

public sealed class EmployeeResponse
{
    public required string FullName { get; init; }
    public required string Guid { get; init; }
}

public static class StaffSearchEndpoint
{
    public static void MapStaffSearchEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/staff", SearchStaff);
    }

    private static async Task<IResult> SearchStaff(
        string filter,
        [FromServices] StudentsEmployeesClient client
    )
    {
        var employees = await client.GetEmployeesAsync(100, 0, filter);

        var staffResponseModel = employees.Select(e => new EmployeeResponse
        {
            Guid = e.Id,
            FullName = e.FullName,
        });

        var response = new SearchStaffResponse { Employees = staffResponseModel };

        return Results.Ok(response);
    }
}
