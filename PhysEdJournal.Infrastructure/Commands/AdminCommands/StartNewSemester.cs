﻿using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

internal sealed partial class StartNewSemesterCommandValidator : ICommandValidator<string>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(string semesterName)
    {
        if (!MyRegex().IsMatch(semesterName))
        {
            return ValidationResult.Create(new SemesterNameValidationException());
        }

        return ValidationResult.Success;
    }

    /// ReSharper disable once CS8795
    [GeneratedRegex(@"\d{4}-\d{4}/\w{5}")]
    private static partial Regex MyRegex();
}

public sealed class StartNewSemesterCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly StartNewSemesterCommandValidator _validator;

    public StartNewSemesterCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new StartNewSemesterCommandValidator();
    }

    public async Task<Result<Unit>> ExecuteAsync(string semesterName)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(semesterName);

        if (validationResult.IsFailed)
        {
            return validationResult.ValidationException;
        }

        var currentSemester = await _applicationContext
            .Semesters.Where(s => s.IsCurrent == true)
            .SingleOrDefaultAsync();

        if (currentSemester is not null)
        {
            if (currentSemester.Name == semesterName)
            {
                return Unit.Default;
            }

            currentSemester.IsCurrent = false;
            _applicationContext.Update(currentSemester);
        }

        var semester = new SemesterEntity { Name = semesterName, IsCurrent = true };

        _applicationContext.Add(semester);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
