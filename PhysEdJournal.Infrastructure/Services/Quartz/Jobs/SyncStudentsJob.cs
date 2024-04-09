using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using Quartz;

namespace PhysEdJournal.Infrastructure.Services.Quartz.Jobs;

public sealed class SyncStudentsJob : IJob
{
    private readonly SyncStudentsCommand _syncStudentsCommand;

    public SyncStudentsJob(SyncStudentsCommand syncStudentsCommand)
    {
        _syncStudentsCommand = syncStudentsCommand;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _syncStudentsCommand.ExecuteAsync(EmptyPayload.Empty);
    }
}
