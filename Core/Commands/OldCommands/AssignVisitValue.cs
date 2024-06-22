using DB;
using PResult;

namespace Core.Commands.OldCommands;

public sealed class AssignVisitValueCommandPayload
{
    public required string GroupName { get; init; }
    public required double NewVisitValue { get; init; }
}

public sealed class AssignVisitValueCommand : ICommand<AssignVisitValueCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public AssignVisitValueCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(AssignVisitValueCommandPayload commandPayload)
    {
        var group = await _applicationContext.Groups.FindAsync(commandPayload.GroupName);

        if (group is null)
        {
            return new GroupNotFoundError();
        }

        group.VisitValue = commandPayload.NewVisitValue;

        _applicationContext.Groups.Update(group);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
