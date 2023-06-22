using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class DeleteCompetitionCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteCompetitionCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<Result<Unit>> ExecuteAsync(string competitionName)
    {
        var comp = await _applicationContext.Competitions.FindAsync(competitionName);

        if (comp is null)
            return new Result<Unit>(new CompetitionNotFoundException(competitionName));

        _applicationContext.Competitions.Remove(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}