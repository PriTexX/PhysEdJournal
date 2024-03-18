using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

file sealed class Student
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

file class UserInfoClient : IDisposable
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

public class SyncStudentsCommand : ICommand<EmptyPayload, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public SyncStudentsCommand(
        IOptions<ApplicationOptions> options,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userInfoServerUrl = options.Value.UserInfoServerURL;
        _pageSize = options.Value.PageSizeToQueryUserInfoServer;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        // Using ServiceLocator here as this command is launched
        // in the background because if we pass ApplicationContext
        // directly through constructor params it will be closed
        // and disposed before command finishes
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await using var applicationContext =
            scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        using var client = new UserInfoClient(_userInfoServerUrl);

        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;

        // We expect to have more than 2000 students
        // so why not to initialize it with this capacity?
        var existingStudentsGuids = new List<string>(2000);

        var dbGroups = await applicationContext.Groups
            .Select(g => g.GroupName)
            .ToDictionaryAsync(g => g);

        var offset = 0;

        while (true)
        {
            var actualStudents = await client.GetStudentsAsync(_pageSize, offset);

            if (actualStudents.Count == 0)
            {
                break;
            }

            offset += _pageSize;

            var studentsGuids = actualStudents.Select(s => s.Guid).ToList();

            existingStudentsGuids.AddRange(studentsGuids);

            var dbStudents = await applicationContext.Students
                .Where(s => studentsGuids.Contains(s.StudentGuid))
                .ToDictionaryAsync(s => s.StudentGuid);

            foreach (var student in actualStudents.Where(s => IsGroupValid(s.Group)))
            {
                if (!dbGroups.ContainsKey(student.Group))
                {
                    applicationContext.Groups.Add(
                        new GroupEntity { GroupName = student.Group, VisitValue = 2.0, }
                    );
                }

                if (!dbStudents.ContainsKey(student.Guid))
                {
                    applicationContext.Students.Add(
                        new StudentEntity
                        {
                            FullName = student.FullName,
                            GroupNumber = student.Group,
                            StudentGuid = student.Guid,
                            CurrentSemesterName = currentSemesterName,
                            Department = student.Department,
                            Course = student.Course,
                        }
                    );
                }
                else
                {
                    var dbStudent = dbStudents[student.Guid];

                    dbStudent.FullName = student.FullName;
                    dbStudent.GroupNumber = student.Group;
                    dbStudent.StudentGuid = student.Guid;
                    dbStudent.Department = student.Department;
                    dbStudent.Course = student.Course;

                    applicationContext.Students.Update(dbStudent);
                }
            }

            await applicationContext.SaveChangesAsync();
        }

        await applicationContext.Students
            .Where(s => !existingStudentsGuids.Contains(s.StudentGuid))
            .ExecuteDeleteAsync();

        return Unit.Default;
    }

    // Cringe name for method that checks
    // that group has PE lessons
    private bool IsGroupValid(string group)
    {
        // Only 2X1 and 2X9 groups have PE lessons
        return group[2] == '1' || group[2] == '9';
    }
}
