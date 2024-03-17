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

public class SyncStudentsCommand : ICommand<EmptyPayload, Unit>
{
    private static readonly List<Student> _allLatestStudents = new();
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
        await using var scope = _serviceScopeFactory.CreateAsyncScope(); // Использую ServiceLocator т.к. команда запускается в бэкграунде и переданный ей ApplicationContext закрывается до завершения работы команды
        await using var applicationContext =
            scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;

        await FetchStudents(_userInfoServerUrl, _pageSize);

        var guids = _allLatestStudents.Select(s => s.Guid).ToList();

        var studentsToDeactivate = await applicationContext.Students
            .Where(s => !guids.Contains(s.StudentGuid))
            .ToListAsync();

        foreach (var stud in studentsToDeactivate)
        {
            stud.IsActive = false;
        }

        var batchSize = 500;
        await HandleLatestStudents(applicationContext, currentSemesterName, batchSize);

        applicationContext.UpdateRange(studentsToDeactivate);
        await applicationContext.SaveChangesAsync();

        return Unit.Default;
    }

    private async Task HandleLatestStudents(
        ApplicationContext applicationContext,
        string semesterName,
        int batchSize
    )
    {
        var offset = 0;
        var hasMoreStudents = true;

        while (hasMoreStudents)
        {
            var studentsBatch = await applicationContext.Students
                .Where(s => s.IsActive == true)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync();

            var guids = studentsBatch.Select(s => s.StudentGuid).ToList();

            if (!studentsBatch.Any())
            {
                hasMoreStudents = false;
            }

            foreach (var stud in _allLatestStudents)
            {
                if (!guids.Contains(stud.Guid))
                {
                    await applicationContext.AddAsync(
                        CreateStudentEntityFromStudentModel(stud, semesterName)
                    );
                }

                var dbStudent = studentsBatch.First(s => s.StudentGuid == stud.Guid);

                if (dbStudent.GroupNumber != stud.Group)
                {
                    dbStudent.GroupNumber = stud.Group;
                }

                if (dbStudent.FullName != stud.FullName)
                {
                    dbStudent.FullName = stud.FullName;
                }

                if (dbStudent.Course != stud.Course)
                {
                    dbStudent.Course = stud.Course;
                }

                if (dbStudent.Department != stud.Department)
                {
                    dbStudent.Department = stud.Department;
                }
            }

            offset += batchSize;
        }
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
                _allLatestStudents?.Add(student);
            }

            if (!response.Data.Students.PageInfo.HasNextPage)
            {
                break;
            }

            skipSize += pageSize;
        }
    }

    private static StudentEntity CreateStudentEntityFromStudentModel(
        Student student,
        string currentSemesterName
    )
    {
        return new StudentEntity
        {
            StudentGuid = student.Guid,
            FullName = student.FullName,
            GroupNumber = student.Group,
            Course = student.Course,
            Department = student.Department,
            CurrentSemesterName = currentSemesterName,
        };
    }
}
