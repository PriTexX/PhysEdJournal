namespace PhysEdJournal.Api.Endpoints.StaffEndpoint;

public interface IStaffInfoClient
{
    public Task<PagedStaffResponse> GetStaffByFilterAsync(string filter, int pageSize);
}