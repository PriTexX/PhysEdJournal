﻿using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class DeleteStandardPointsCommandPayload
{
    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IsAdmin { get; init; } = false;
}

public sealed class DeleteStandardPointsCommand : ICommand<DeleteStandardPointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteStandardPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(DeleteStandardPointsCommandPayload commandPayload)
    {
        var history = await _applicationContext.StandardsStudentsHistory.FirstOrDefaultAsync(
            s => s.Id == commandPayload.HistoryId
        );

        if (history is null)
        {
            return new StandardsStudentHistoryNotFoundException(commandPayload.HistoryId);
        }

        if (commandPayload.TeacherGuid != history.TeacherGuid && !commandPayload.IsAdmin)
        {
            return new TeacherGuidMismatchException(history.TeacherGuid);
        }

        var student = await _applicationContext.Students.FirstAsync(
            s => s.StudentGuid == history.StudentGuid
        );

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - history.Date.DayNumber
                > DAYS_TO_DELETE_POINTS
            && !commandPayload.IsAdmin
        )
        {
            return new PointsOutdatedException(DAYS_TO_DELETE_POINTS);
        }

        var totalPointsForStandards = await _applicationContext.StandardsStudentsHistory
            .Where(h => h.StudentGuid == student.StudentGuid)
            .SumAsync(h => h.Points);

        var pointsForStandard = totalPointsForStandards - history.Points;
        var limitedPoints =
            pointsForStandard > MAX_POINTS_FOR_STANDARDS
                ? MAX_POINTS_FOR_STANDARDS
                : pointsForStandard;

        student.PointsForStandards = limitedPoints;

        _applicationContext.StandardsStudentsHistory.Remove(history);

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
