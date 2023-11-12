using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Core.Constants.VisitConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class DeleteStudentVisitCommandPayload
{
    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IsAdmin { get; init; } = false;
}

public sealed class DeleteStudentVisitCommand : ICommand<DeleteStudentVisitCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteStudentVisitCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(
        DeleteStudentVisitCommandPayload commandCommandPayload
    )
    {
        var history = await _applicationContext.VisitsStudentsHistory.FirstOrDefaultAsync(
            s => s.Id == commandCommandPayload.HistoryId
        );

        if (history is null)
        {
            return new Result<Unit>(
                new VisitsStudentHistoryNotFoundException(commandCommandPayload.HistoryId)
            );
        }

        if (
            commandCommandPayload.TeacherGuid != history.TeacherGuid
            && !commandCommandPayload.IsAdmin
        )
        {
            return new Result<Unit>(new TeacherGuidMismatchException(history.TeacherGuid));
        }

        var student = await _applicationContext.Students.FirstAsync(
            s => s.StudentGuid == history.StudentGuid
        );

        if (history.IsArchived)
        {
            return new Result<Unit>(new ArchivedVisitDeletionException());
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - history.Date.DayNumber
                > DAYS_TO_DELETE_VISIT
            && !commandCommandPayload.IsAdmin
        )
        {
            return new Result<Unit>(new VisitOutdatedException(DAYS_TO_DELETE_VISIT));
        }

        student.Visits--;
        _applicationContext.VisitsStudentsHistory.Remove(history);
        _applicationContext.Students.Update(student);

        try
        {
            await _applicationContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new Result<Unit>(new ConcurrencyError());
        }

        return Unit.Default;
    }
}
