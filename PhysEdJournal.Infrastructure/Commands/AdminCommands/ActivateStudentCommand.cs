﻿using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class ActivateStudentCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public ActivateStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(string studentGuid)
    {
        var rowsAffected = await _applicationContext.Students
            .Where(s => s.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsActive, true));

        if (rowsAffected == 0)
        {
            return new Result<Unit>(new StudentNotFoundException(studentGuid));
        }

        return Unit.Default;
    }
}
