﻿using DB;
using PResult;

namespace Core.Commands;

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
        {
            return new CompetitionNotFoundError();
        }

        _applicationContext.Competitions.Remove(comp);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
