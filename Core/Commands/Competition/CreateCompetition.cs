using DB;
using DB.Tables;
using PResult;

namespace Core.Commands;

public sealed class CreateCompetitionCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public CreateCompetitionCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(string competitionName)
    {
        var comp = new CompetitionEntity { CompetitionName = competitionName };

        _applicationContext.Competitions.Add(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
