using System.Text;
using GraphQL;
using Newtonsoft.Json;
using PhysEdJournal.Infrastructure.Models;

namespace PhysEdJournal.Infrastructure.Services.StaticFunctions;

public static class TeacherServiceFunctions
{
    public static async IAsyncEnumerable<Teacher> GetAllTeachersAsync(string url, int pageSize)
    {
        var client = new HttpClient();
        var skipSize = 0;

        while (true)
        {
            var query = $"{{\"query\":\"{{ employees(take: {pageSize}, skip: {skipSize},filter: \\\"employeeEmployments.Any(subdivision.Contains(\\\\\\\"Физическое\\\\\\\") && jobState == \\\\\\\"Работа\\\\\\\")\\\"){{ hasNextPage items{{ fullName guid mail }}}}}}\"}}";
            var content = new StringContent(query, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error {response.StatusCode}: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var graphQLResponse = JsonConvert.DeserializeObject<GraphQLResponse<PagedGraphQLTeacher>>(responseContent);

            if (graphQLResponse.Errors != null && graphQLResponse.Errors.Any())
            {
                var errors = graphQLResponse.Errors.Select(e => new Exception(e.Message)).ToList();
                throw new AggregateException(errors);
            }

            foreach (var employee in graphQLResponse.Data.Employees.Items)
            {
                yield return employee;
            }

            if (!graphQLResponse.Data.Employees.HasNextPage)
                break;

            skipSize += pageSize;
        }
    }
}