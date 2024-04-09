using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands;

public class SyncStudentsCommand : ICommand<EmptyPayload, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SyncStudentsCommand> _logger;
    private readonly string _userInfoServerUrl;
    private readonly int _pageSize;

    public SyncStudentsCommand(
        IOptions<ApplicationOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SyncStudentsCommand> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
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

        _logger.LogInformation($"Starting {nameof(SyncStudentsCommand)}");

        var currentSemesterName = (await applicationContext.GetActiveSemester()).Name;

        // We expect to have more than 4000 students
        // so why not to initialize it with this capacity?
        var existingStudentsGuids = new List<string>(4000);

        var dbGroups = await applicationContext
            .Groups.Select(g => g.GroupName)
            .ToDictionaryAsync(g => g, _ => true);

        var offset = 0;

        while (true)
        {
            var actualStudents = await client.GetStudentsAsync(_pageSize, offset);

            if (actualStudents.Count == 0)
            {
                break;
            }

            offset += _pageSize;

            _logger.LogInformation(
                "Received {chunkNum} chunk of students from `userinfo`",
                offset / _pageSize
            );

            var studentsGuids = actualStudents.Select(s => s.Guid).ToList();

            var dbStudents = await applicationContext
                .Students.Where(s => studentsGuids.Contains(s.StudentGuid))
                .ToDictionaryAsync(s => s.StudentGuid);

            foreach (var student in actualStudents.Where(StudentHasPELessons))
            {
                existingStudentsGuids.Add(student.Guid);

                if (!dbGroups.ContainsKey(student.Group))
                {
                    _logger.LogInformation("Adding new group: {groupName}", student.Group);

                    dbGroups.Add(student.Group, true);
                    applicationContext.Groups.Add(
                        new GroupEntity { GroupName = student.Group, VisitValue = 2.0, }
                    );
                }

                if (dbStudents.TryGetValue(student.Guid, out var dbStudent))
                {
                    dbStudent.FullName = student.FullName;
                    dbStudent.GroupNumber = student.Group;
                    dbStudent.StudentGuid = student.Guid;
                    dbStudent.Department = student.Department;
                    dbStudent.Course = student.Course;

                    applicationContext.Students.Update(dbStudent);
                }
                else
                {
                    _logger.LogInformation("Adding new student: {studentGuid}", student.Guid);

                    applicationContext.Students.Add(
                        new StudentEntity
                        {
                            FullName = student.FullName,
                            GroupNumber = student.Group,
                            StudentGuid = student.Guid,
                            CurrentSemesterName = currentSemesterName,
                            Department = student.Department,
                            Course = student.Course,
                            IsActive = true,
                        }
                    );
                }
            }

            await applicationContext.SaveChangesAsync();
        }

        await applicationContext
            .Students.Where(s => !existingStudentsGuids.Contains(s.StudentGuid))
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsActive, false));

        _logger.LogInformation($"Finished {nameof(SyncStudentsCommand)}");

        return Unit.Default;
    }

    private bool StudentHasPELessons(Student s)
    {
        // Only 2X1 and 2X9 groups have PE lessons
        return s.Group[2] == '1' || s.Group[2] == '9';
    }
}
