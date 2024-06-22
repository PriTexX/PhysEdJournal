using Core.Cfg;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace Admin.Api.StaffSearch;

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

public sealed class StaffHttpClient
{
    private readonly GraphQLHttpClient _graphQlClient;

    public StaffHttpClient()
    {
        _graphQlClient = new GraphQLHttpClient(
            Config.UserInfoServerURL,
            new NewtonsoftJsonSerializer()
        );
    }

    public async Task<PagedStaffResponse> GetStaffByFilterAsync(string filter)
    {
        var query =
            @"query($pageSize: Int!, $filter: String!) {
            employees(
              take: $pageSize,
              order: { fullName: ASC },
              where: { fullName: { contains: $filter } }
            ) {
              items {
                fullName
                guid
              }
            }
        }";

        var request = new GraphQLRequest
        {
            Query = query,
            Variables = new { pageSize = 200, filter },
        };

        var response = await _graphQlClient.SendQueryAsync<PagedStaffResponse>(request);

        return response.Data;
    }
}
