using DB;
using PResult;

namespace Core.Commands.OldCommands;

public sealed class AssignCuratorCommandPayload
{
    public required string GroupName { get; init; }
    public required string TeacherGuid { get; init; }
}

public sealed class AssignCuratorCommand : ICommand<AssignCuratorCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public AssignCuratorCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(AssignCuratorCommandPayload commandPayload)
    {
        var teacher = await _applicationContext.Teachers.FindAsync(commandPayload.TeacherGuid);

        if (teacher is null)
        {
            return new TeacherNotFoundError();
        }

        var group = await _applicationContext.Groups.FindAsync(commandPayload.GroupName);

        if (group is null)
        {
            return new GroupNotFoundError();
        }

        group.Curator = teacher;
        group.CuratorGuid = teacher.TeacherGuid;

        _applicationContext.Groups.Update(group);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
