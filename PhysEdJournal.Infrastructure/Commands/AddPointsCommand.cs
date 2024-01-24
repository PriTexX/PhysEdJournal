﻿using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Constants;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddPointsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required WorkType WorkType { get; init; }
    public required bool IsAdmin { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddPointsCommandValidator : ICommandValidator<AddPointsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddPointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        AddPointsCommandPayload commandInput
    )
    {
        if (commandInput.Points > MaxPointsAmount)
        {
            return new PointsExceededLimit(MaxPointsAmount);
        }

        if (commandInput.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ActionFromFutureException(commandInput.Date);
        }

        if (commandInput.Points <= 0)
        {
            return new NegativePointAmount();
        }

        var student = await _applicationContext.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.StudentGuid == commandInput.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundException(commandInput.StudentGuid);
        }

        if (commandInput.WorkType == WorkType.ExternalFitness)
        {
            var anotherFitness = await _applicationContext.PointsStudentsHistory
                .AsNoTracking()
                .Where(
                    p =>
                        p.StudentGuid == commandInput.StudentGuid
                        && p.WorkType == WorkType.ExternalFitness
                )
                .FirstOrDefaultAsync();

            if (anotherFitness is not null)
            {
                return new FitnessAlreadyExistsException();
            }

            if (commandInput.Points > MaxPointsForExternalFitness)
            {
                return new PointsExceededLimit(MaxPointsForExternalFitness);
            }
        }

        if (commandInput.WorkType == WorkType.GTO)
        {
            var anotherGTO = await _applicationContext.PointsStudentsHistory
                .AsNoTracking()
                .Where(p => p.StudentGuid == commandInput.StudentGuid && p.WorkType == WorkType.GTO)
                .FirstOrDefaultAsync();

            if (anotherGTO is not null)
            {
                return new GTOAlreadyExistsException();
            }
        }

        if (commandInput is { WorkType: WorkType.Science, Points: > MaxPointsForScience })
        {
            return new PointsExceededLimit(MaxPointsForScience);
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - commandInput.Date.DayNumber
                > PointsConstants.POINTS_LIFE_DAYS
            && !commandInput.IsAdmin
        )
        {
            return new DateExpiredException(commandInput.Date);
        }

        if (commandInput.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayException(commandInput.Date.DayOfWeek);
        }

        return ValidationResult.Success;
    }
}

public sealed class AddPointsCommand : ICommand<AddPointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddPointsCommandValidator _validator;

    public AddPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddPointsCommandValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddPointsCommandPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await _applicationContext.Students
            .Include(s => s.Group)
            .FirstAsync(s => s.StudentGuid == commandPayload.StudentGuid);

        var pointsStudentHistoryEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            Comment = commandPayload.Comment,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            WorkType = commandPayload.WorkType,
            TeacherGuid = commandPayload.TeacherGuid,
        };

        student.AdditionalPoints += pointsStudentHistoryEntity.Points;

        _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
        _applicationContext.Students.Update(student);

        try
        {
            await _applicationContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConcurrencyError();
        }

        return Unit.Default;
    }
}
