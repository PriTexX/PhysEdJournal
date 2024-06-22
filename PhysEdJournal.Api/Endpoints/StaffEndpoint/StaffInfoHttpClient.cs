using Core.Cfg;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace PhysEdJournal.Api.Endpoints.StaffEndpoint;

public sealed class StaffInfoHttpClient : IStaffInfoClient
{
    private readonly GraphQLHttpClient _graphQlClient;

    public StaffInfoHttpClient()
    {
        _graphQlClient = new GraphQLHttpClient(
            Config.UserInfoServerURL,
            new NewtonsoftJsonSerializer()
        );
    }

    public async Task<PagedStaffResponse> GetStaffByFilterAsync(string filter, int pageSize)
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

        var request = new GraphQLRequest { Query = query, Variables = new { pageSize, filter } };
        var response = await _graphQlClient.SendQueryAsync<PagedStaffResponse>(request);

        return response.Data;
    }
}
