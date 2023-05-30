using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class CreateCompetitionCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public CreateCompetitionCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<Result<Unit>> ExecuteAsync(string competitionName)
    {
        var comp = new CompetitionEntity{CompetitionName = competitionName};

        _applicationContext.Competitions.Add(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}