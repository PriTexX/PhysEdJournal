﻿using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class DeletePointsCommandPayload
{
    public required string StudentGuid { get; init; }

    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IgnoreDateIntervalCheck { get; init; } = false;
}

public sealed class DeletePointsCommand : ICommand<DeletePointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeletePointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(DeletePointsCommandPayload commandPayload)
    {
        var student = await _applicationContext.Students.FirstOrDefaultAsync(
            s => s.StudentGuid == commandPayload.StudentGuid
        );

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
        }

        var history = await _applicationContext.PointsStudentsHistory.FirstOrDefaultAsync(
            s => s.Id == commandPayload.HistoryId
        );

        if (history is null)
        {
            return new Result<Unit>(
                new PointsStudentHistoryNotFoundException(
                    commandPayload.HistoryId,
                    commandPayload.StudentGuid
                )
            );
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - history.Date.DayNumber
                > DAYS_TO_DELETE_POINTS
            && !commandPayload.IgnoreDateIntervalCheck
        )
        {
            return new Result<Unit>(new PointsOutdatedException(DAYS_TO_DELETE_POINTS));
        }

        student.AdditionalPoints -= history.Points;
        _applicationContext.PointsStudentsHistory.Remove(history);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
