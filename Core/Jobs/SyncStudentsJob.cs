using Coravel.Invocable;
using Core.Commands;
using Core.Commands.SyncStudents;

namespace Core.Jobs;

public sealed class SyncStudentsJob : IInvocable
{
    private readonly SyncStudentsCommand _syncStudentsCommand;

    public SyncStudentsJob(SyncStudentsCommand syncStudentsCommand)
    {
        _syncStudentsCommand = syncStudentsCommand;
    }

    public Task Invoke()
    {
        return _syncStudentsCommand.ExecuteAsync(EmptyPayload.Empty);
    }
}
