using Core.Commands.SyncStudents;
using Core.Config;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PResult;
using Serilog;

namespace Core.Commands;

public class SyncStudentsCommand : ICommand<EmptyPayload, Unit>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SyncStudentsCommand> _logger;

    public SyncStudentsCommand(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SyncStudentsCommand> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
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

        var client = scope.ServiceProvider.GetRequiredService<StudentsEmployeesClient>();

        _logger.LogInformation($"Starting {nameof(SyncStudentsCommand)}");

        var currentSemesterName = (
            await applicationContext.Semesters.SingleAsync(s => s.IsCurrent)
        ).Name;

        var existingStudentsGuids = new List<string>();

        var dbGroups = await applicationContext
            .Groups.Select(g => g.GroupName)
            .ToDictionaryAsync(g => g, _ => true);

        var offset = 0;

        while (true)
        {
            var actualStudents = await client.GetStudentsAsync(Cfg.PageSizeToQueryStudents, offset);

            if (actualStudents.Count == 0)
            {
                break;
            }

            offset += Cfg.PageSizeToQueryStudents;

            _logger.LogInformation(
                "Received {chunkNum} chunk of students from `userinfo`",
                offset / Cfg.PageSizeToQueryStudents
            );

            var studentsGuids = actualStudents.Select(s => s.Guid).ToList();

            var dbStudents = await applicationContext
                .Students.Where(s => studentsGuids.Contains(s.StudentGuid))
                .ToDictionaryAsync(s => s.StudentGuid);

            foreach (var student in actualStudents)
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
                    dbStudent.IsActive = true;

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

                try
                {
                    await applicationContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to sync student: {studentGuid}", student.Guid);

                    applicationContext.ChangeTracker.Clear();
                }
            }
        }

        await applicationContext
            .Students.Where(s => !existingStudentsGuids.Contains(s.StudentGuid))
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsActive, false));

        _logger.LogInformation($"Finished {nameof(SyncStudentsCommand)}");

        return Unit.Default;
    }
}
