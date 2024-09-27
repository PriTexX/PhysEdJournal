using DB;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class ClearStudentPayload
{
    public required string StudentGuid { get; init; }
}

public sealed class ClearStudentCommand : ICommand<ClearStudentPayload, Unit>
{
    private readonly ApplicationContext _appCtx;

    public ClearStudentCommand(ApplicationContext appCtx)
    {
        _appCtx = appCtx;
    }

    public async Task<Result<Unit>> ExecuteAsync(ClearStudentPayload payload)
    {
        var activeSemester = await _appCtx.Semesters.SingleAsync(s => s.IsCurrent);
        var student = await _appCtx
            .Students.Include(s => s.VisitsHistory)
            .Include(s => s.StandardsHistory)
            .Include(s => s.PointsHistory)
            .FirstOrDefaultAsync(s => s.StudentGuid == payload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        student.CurrentSemesterName = activeSemester.Name;

        student.Visits = 0;
        student.PointsForStandards = 0;
        student.AdditionalPoints = 0;

        student.VisitsHistory!.Clear();
        student.StandardsHistory!.Clear();
        student.PointsHistory!.Clear();

        student.HasDebt = false;
        student.HadDebtInSemester = false;

        student.ArchivedVisitValue = 0;

        await _appCtx.SaveChangesAsync();

        return Unit.Default;
    }
}
