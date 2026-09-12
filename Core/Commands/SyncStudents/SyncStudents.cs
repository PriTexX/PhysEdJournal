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

file sealed class Student
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
    public required string Group { get; set; }
    public required string Department { get; set; }
    public required int Course { get; set; }
}

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
            var studentsChunk = await client.GetStudentsAsync(Cfg.PageSizeToQueryStudents, offset);

            if (studentsChunk.Count == 0)
            {
                break;
            }

            offset += Cfg.PageSizeToQueryStudents;

            _logger.LogInformation(
                "Received {chunkNum} chunk of students from `userinfo`",
                offset / Cfg.PageSizeToQueryStudents
            );

            var actualStudents = studentsChunk
                .Where(s =>
                    s.Educations.Any(e =>
                        e.IsStudying && e.Group != string.Empty && StudentHasPELessons(e)
                    )
                )
                .Select(s =>
                {
                    var education = s
                        .Educations.Where(e =>
                            e.IsStudying && e.Group != string.Empty && StudentHasPELessons(e)
                        )
                        .OrderByDescending(e => e.StartYear)
                        .First();

                    return new Student
                    {
                        Guid = s.Id,
                        FullName = s.FullName,
                        Group = education.Group,
                        Course = education.Course,
                        Department = education.Department,
                    };
                })
                .ToList();

            var studentsGuids = actualStudents.Select(s => s.Guid).ToList();

            var dbStudents = await applicationContext
                .Students.AsNoTracking()
                .Where(s => studentsGuids.Contains(s.StudentGuid))
                .ToDictionaryAsync(s => s.StudentGuid);

            foreach (var student in actualStudents)
            {
                try
                {
                    applicationContext.ChangeTracker.Clear();

                    existingStudentsGuids.Add(student.Guid);

                    if (!dbGroups.ContainsKey(student.Group))
                    {
                        _logger.LogInformation("Adding new group: {groupName}", student.Group);

                        dbGroups.Add(student.Group, true);
                        applicationContext.Groups.Add(
                            new GroupEntity { GroupName = student.Group, VisitValue = 2.0, }
                        );

                        await applicationContext.SaveChangesAsync();
                    }

                    if (dbStudents.TryGetValue(student.Guid, out var dbStudent))
                    {
                        await applicationContext
                            .Students.Where(s => s.StudentGuid == student.Guid)
                            .ExecuteUpdateAsync(p =>
                                p.SetProperty(s => s.FullName, student.FullName)
                                    .SetProperty(s => s.GroupNumber, student.Group)
                                    .SetProperty(s => s.Department, student.Department)
                                    .SetProperty(s => s.Course, student.Course)
                                    .SetProperty(s => s.IsActive, true)
                            );
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

                        await applicationContext.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to sync student: {studentGuid}", student.Guid);
                }
            }
        }

        await applicationContext
            .Students.Where(s => !existingStudentsGuids.Contains(s.StudentGuid))
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsActive, false));

        _logger.LogInformation($"Finished {nameof(SyncStudentsCommand)}");

        return Unit.Default;
    }

    private bool StudentHasPELessons(StudentEducation s)
    {
        // Only 2X1 and 2X9 groups have PE lessons
        return s.Group[2] == '1' || s.Group[2] == '9';
    }
}
