using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class DeleteCompetitionCommandPayload
{
    public required string CompetitionName { get; init; }
}

public sealed class DeleteCompetitionCommand : ICommand<DeleteCompetitionCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteCompetitionCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<Result<Unit>> ExecuteAsync(DeleteCompetitionCommandPayload commandPayload)
    {
        var comp = await _applicationContext.Competitions.FindAsync(commandPayload.CompetitionName);

        if (comp is null)
            return new Result<Unit>(new CompetitionNotFoundException(commandPayload.CompetitionName));

        _applicationContext.Competitions.Remove(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}