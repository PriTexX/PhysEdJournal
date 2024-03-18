using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace PhysEdJournal.Infrastructure.Commands;

internal sealed class Student
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
    public required string Group { get; set; }
    public required int Course { get; set; }
    public required string Department { get; set; }
}

file sealed class Students
{
    public required List<Student> Items { get; set; }
}

file sealed class PagedGraphQLStudent
{
    public required Students Students { get; set; }
}

internal sealed class UserInfoClient : IDisposable
{
    private readonly GraphQLHttpClient _client;

    public UserInfoClient(string userInfoServerUrl)
    {
        _client = new GraphQLHttpClient(userInfoServerUrl, new NewtonsoftJsonSerializer());
    }

    public async Task<List<Student>> GetStudentsAsync(int limit, int offset)
    {
        var query =
            @"query($pageSize: Int!, $skipSize: Int!) {
            students(take: $pageSize, skip: $skipSize, where: {group: {neq: """"}}) {
                items {
                    guid
                    fullName
                    group
                    course
                    department
                }
            }
         }";

        var request = new GraphQLRequest { Query = query, Variables = new { limit, offset }, };

        var response = await _client.SendQueryAsync<PagedGraphQLStudent>(request);

        if (response.Errors != null && response.Errors.Any())
        {
            var errors = response.Errors.Select(e => new Exception(e.Message)).ToList();
            throw new AggregateException(errors);
        }

        return response.Data.Students.Items;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
