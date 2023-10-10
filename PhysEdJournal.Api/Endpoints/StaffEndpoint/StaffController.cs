using Microsoft.AspNetCore.Mvc;

namespace PhysEdJournal.Api.Endpoints.StaffEndpoint;

[ApiController]
[Route("[controller]")]
public sealed class StaffController : ControllerBase
{
    private readonly IStaffInfoClient _staffInfoClient;

    public StaffController(IStaffInfoClient staffInfoClient)
    {
        _staffInfoClient = staffInfoClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetStaffByFilterAsync(
        [FromQuery] GetStaffByFilterRequest request
    )
    {
        var staff = await _staffInfoClient.GetStaffByFilterAsync(request.Filter, request.PageSize);

        var staffResponseModel = staff.Employees.Items.Select(
            e => new EmployeeResponse { FullName = e.FullName, Guid = e.Guid }
        );
        var response = new GetStaffByFilterResponse { Employees = staffResponseModel };

        return new OkObjectResult(response);
    }
}
