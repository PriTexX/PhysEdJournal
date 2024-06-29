using Core.Config;
using DB;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class DeleteVisitPayload
{
    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IsAdminOrSecretary { get; init; }
}

public sealed class DeleteVisitCommand : ICommand<DeleteVisitPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteVisitCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(DeleteVisitPayload payload)
    {
        var history = await _applicationContext.VisitsStudentsHistory.FirstOrDefaultAsync(s =>
            s.Id == payload.HistoryId
        );

        if (history is null)
        {
            return new HistoryNotFoundError();
        }

        if (payload.TeacherGuid != history.TeacherGuid && !payload.IsAdminOrSecretary)
        {
            return new TeacherMismatchError();
        }

        var student = await _applicationContext.Students.FirstAsync(s =>
            s.StudentGuid == history.StudentGuid
        );

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - history.Date.DayNumber
                > Cfg.DaysToDeleteVisit
            && !payload.IsAdminOrSecretary
        )
        {
            return new HistoryDeleteExpiredError();
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
            return new ConcurrencyError();
        }

        return Unit.Default;
    }
}
