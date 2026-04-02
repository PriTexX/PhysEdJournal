using Coravel.Invocable;
using Core.Commands;
using Core.Commands.SyncStudents;
using Serilog;

namespace Core.Jobs;

public sealed class SyncStudentsJob : IInvocable
{
    private readonly SyncStudentsCommand _syncStudentsCommand;

    public SyncStudentsJob(SyncStudentsCommand syncStudentsCommand)
    {
        _syncStudentsCommand = syncStudentsCommand;
    }

    public async Task Invoke()
    {
        try
        {
            await _syncStudentsCommand.ExecuteAsync(EmptyPayload.Empty);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed StudentSync command");
        }
    }
}
