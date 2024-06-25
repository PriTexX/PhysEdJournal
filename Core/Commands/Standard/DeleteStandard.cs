using Core.Cfg;
using DB;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class DeleteStandardPayload
{
    public required int HistoryId { get; init; }

    public required string TeacherGuid { get; init; }

    public required bool IsAdminOrSecretary { get; init; } = false;
}

public sealed class DeleteStandardCommand : ICommand<DeleteStandardPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeleteStandardCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(DeleteStandardPayload commandPayload)
    {
        var history = await _applicationContext.StandardsStudentsHistory.FirstOrDefaultAsync(s =>
            s.Id == commandPayload.HistoryId
        );

        if (history is null)
        {
            return new HistoryNotFoundError();
        }

        if (commandPayload.TeacherGuid != history.TeacherGuid && !commandPayload.IsAdminOrSecretary)
        {
            return new TeacherMismatchError();
        }

        var student = await _applicationContext.Students.FirstAsync(s =>
            s.StudentGuid == history.StudentGuid
        );

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - history.Date.DayNumber
                > Config.DaysToDeletePoints
            && !commandPayload.IsAdminOrSecretary
        )
        {
            return new HistoryDeleteExpiredError();
        }

        var totalPointsForStandards = await _applicationContext
            .StandardsStudentsHistory.Where(h => h.StudentGuid == student.StudentGuid)
            .SumAsync(h => h.Points);

        var pointsForStandard = totalPointsForStandards - history.Points;
        var limitedPoints =
            pointsForStandard > Config.MaxPointsForStandards
                ? Config.MaxPointsForStandards
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
