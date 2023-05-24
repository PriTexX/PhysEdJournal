using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class CreateCompetitionCommandPayload
{
    public required string CompetitionName { get; init; }
}

public sealed class CreateCompetitionCommand : ICommand<CreateCompetitionCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public CreateCompetitionCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<Result<Unit>> ExecuteAsync(CreateCompetitionCommandPayload commandPayload)
    {
        var comp = new CompetitionEntity{CompetitionName = commandPayload.CompetitionName};

        _applicationContext.Competitions.Add(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}