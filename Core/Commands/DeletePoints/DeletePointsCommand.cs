using Core.Cfg;
using DB;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class DeletePointsPayload
{
    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IsAdminOrSecretary { get; init; } = false;
}

public sealed class DeletePointsCommand : ICommand<DeletePointsPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeletePointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(DeletePointsPayload payload)
    {
        var history = await _applicationContext.PointsStudentsHistory.FirstOrDefaultAsync(s =>
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
                > Config.DaysToDeletePoints
            && !payload.IsAdminOrSecretary
        )
        {
            return new HistoryDeleteExpiredError();
        }

        student.AdditionalPoints -= history.Points;
        _applicationContext.PointsStudentsHistory.Remove(history);
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
