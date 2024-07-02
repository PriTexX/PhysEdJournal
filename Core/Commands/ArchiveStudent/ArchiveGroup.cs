using Core.Dtos;
using DB;
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
    : ICommand<ArchiveGroupPayload, IEnumerable<ArchiveStudentStatusDto>>
{
    private readonly ApplicationContext _applicationContext;
    private readonly ArchiveStudentCommand _command;

    public ArchiveGroupCommand(ApplicationContext applicationContext, ArchiveStudentCommand command)
    {
        _applicationContext = applicationContext;
        _command = command;
    }

    public async Task<Result<IEnumerable<ArchiveStudentStatusDto>>> ExecuteAsync(
        ArchiveGroupPayload commandPayload
    )
    {
        var group = await _applicationContext
            .Groups.Where(g => g.GroupName == commandPayload.GroupName)
            .Include(g => g.Students)
            .FirstOrDefaultAsync();

        if (group is null)
        {
            return new GroupNotFoundError();
        }

        if (group.Students is null || group.Students.Count == 0)
        {
            return new NoStudentsInGroupError();
        }

        var students = new List<ArchiveStudentStatusDto>();

        foreach (var stud in group.Students)
        {
            var payload = new ArchiveStudentPayload
            {
                StudentGuid = stud.StudentGuid,
                TeacherGuid = commandPayload.TeacherGuid,
                IsAdmin = commandPayload.IsAdmin,
            };

            var result = await _command.ExecuteAsync(payload);

            var studDto = new ArchiveStudentStatusDto
            {
                IsArchived = result.IsOk,
                Guid = stud.StudentGuid,
                FullName = stud.FullName,
                Error = result.IsErr ? result.UnsafeError : null,
            };

            students.Add(studDto);
        }

        return students;
    }
}
