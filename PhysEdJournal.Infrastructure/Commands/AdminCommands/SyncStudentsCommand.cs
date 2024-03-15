using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public class SyncStudentsCommand : ICommand<EmptyPayload, Unit>
{
    private static List<Student> _allUpdatedStudents = new();
    private readonly ApplicationContext _applicationContext;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public SyncStudentsCommand(
        IOptions<ApplicationOptions> options,
        ApplicationContext applicationContext
    )
    {
        _applicationContext = applicationContext;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        await FetchStudents(_userInfoServerUrl, _pageSize);

        var guids = _allUpdatedStudents.Select(s => s.Guid).ToList();

        var studentsToDeactivate = await _applicationContext.Students
            .Where(s => !guids.Contains(s.StudentGuid)) // здесь "Id" - это свойство, по которому вы хотите выполнить фильтрацию
            .ToListAsync();

        foreach (var stud in studentsToDeactivate)
        {
            stud.IsActive = false;
        }

        _applicationContext.UpdateRange(studentsToDeactivate);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }

    private async Task FetchStudents(string url, int pageSize)
    {
        var query =
            @"query($pageSize: Int!, $skipSize: Int!) {
            students(take: $pageSize, skip: $skipSize, where: {group: {neq: """"}}) {
                pageInfo{hasNextPage}
                items {
                    guid
                    fullName
                    group
                    course
                    department
                }
            }
         }";

        var client = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
        var skipSize = 0;

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new { pageSize, skipSize },
            };
            var response = await client.SendQueryAsync<PagedGraphQLStudent>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                var errors = response.Errors.Select(e => new Exception(e.Message)).ToList();
                throw new AggregateException(errors);
            }

            foreach (var student in response.Data.Students.Items)
            {
                _allUpdatedStudents?.Add(student);
            }

            if (!response.Data.Students.PageInfo.HasNextPage)
            {
                break;
            }

            skipSize += pageSize;
        }
    }
}
