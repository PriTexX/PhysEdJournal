using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class ClearStudentsHealthGroupCommand : ICommand<EmptyPayload, Unit>
{
    private readonly ApplicationContext _appContext;

    public ClearStudentsHealthGroupCommand(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(EmptyPayload commandPayload)
    {
        await _appContext.Students.ExecuteUpdateAsync(p =>
            p.SetProperty(s => s.HealthGroup, HealthGroupType.None)
        );

        return Unit.Default;
    }
}
