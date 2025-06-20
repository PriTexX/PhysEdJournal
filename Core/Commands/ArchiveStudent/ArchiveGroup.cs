using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class ArchiveGroupPayload
{
    public required string GroupName { get; init; }
    public string? TeacherGuid { get; init; }
    public required bool IsAdmin { get; init; }
}

public sealed class ArchiveGroupCommand
    : ICommand<ArchiveGroupPayload, IEnumerable<ArchiveStudentStatus>>
{
    private readonly ApplicationContext _applicationContext;
    private readonly ArchiveStudentCommand _command;

    public ArchiveGroupCommand(ApplicationContext applicationContext, ArchiveStudentCommand command)
    {
        _applicationContext = applicationContext;
        _command = command;
    }

    public async Task<Result<IEnumerable<ArchiveStudentStatus>>> ExecuteAsync(
        ArchiveGroupPayload commandPayload
    )
    {
        var currentSemester = await _applicationContext.Semesters.SingleAsync(sem => sem.IsCurrent);

        var group = await _applicationContext
            .Students.Where(s =>
                s.GroupNumber == commandPayload.GroupName
                && s.CurrentSemesterName != currentSemester.Name
            )
            .ToListAsync();

        var tasks = new List<Task<ArchiveStudentStatus>>();
        foreach (var stud in group)
        {
            var payload = new ArchiveStudentPayload
            {
                StudentGuid = stud.StudentGuid,
                TeacherGuid = commandPayload.TeacherGuid,
                IsAdmin = commandPayload.IsAdmin,
            };

            var task = ArchiveOneAsync(payload, stud.StudentGuid);

            tasks.Add(task);
        }

        return await Task.WhenAll(tasks);
    }

    private async Task<ArchiveStudentStatus> ArchiveOneAsync(
        ArchiveStudentPayload payload,
        string fullName
    )
    {
        try
        {
            var result = await _command.ExecuteAsync(payload);

            return new ArchiveStudentStatus
            {
                IsArchived = result.IsOk,
                Guid = payload.StudentGuid,
                FullName = fullName,
                Error = result.IsErr ? result.UnsafeError : null,
            };
        }
        catch (Exception e)
        {
            return new ArchiveStudentStatus
            {
                IsArchived = false,
                Guid = payload.StudentGuid,
                FullName = fullName,
                Error = e,
            };
        }
    }
}

public sealed class ArchiveStudentStatus
{
    public required bool IsArchived { get; set; }
    public required string Guid { get; set; }
    public required string FullName { get; set; }
    public Exception? Error { get; set; }
}
