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
        [FromServices] StaffHttpClient client
    )
    {
        var staff = await client.GetStaffByFilterAsync(filter);

        var staffResponseModel = staff.Employees.Items.Select(e => new EmployeeResponse
        {
            FullName = e.FullName,
            Guid = e.Guid,
        });

        var response = new SearchStaffResponse { Employees = staffResponseModel };

        return Results.Ok(response);
    }
}
