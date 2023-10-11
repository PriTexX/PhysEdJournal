using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Services;

internal sealed class StudentArchiver
{
    private readonly ApplicationContext _applicationContext;

    public StudentArchiver(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<ArchivedStudentEntity> ArchiveStudentAsync(
        InternalArchiveStudentPayload student
    )
    {
        await using var transaction = await _applicationContext.Database.BeginTransactionAsync();

        await _applicationContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .ExecuteUpdateAsync(
                p => p.SetProperty(s => s.CurrentSemesterName, student.ActiveSemesterName)
            );

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = student.TotalPoints,
            Visits = student.Visits,
            SemesterName = student.CurrentSemesterName,
        };

        _applicationContext.ArchivedStudents.Add(archivedStudent);
        await _applicationContext.SaveChangesAsync();

        await ArchiveCurrentSemesterHistoryAsync(student.StudentGuid, student.CurrentSemesterName);

        await _applicationContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(s => s.AdditionalPoints, 0)
                        .SetProperty(s => s.Visits, 0)
                        .SetProperty(s => s.PointsForStandards, 0)
                        .SetProperty(s => s.HasDebtFromPreviousSemester, false)
                        .SetProperty(s => s.ArchivedVisitValue, 0)
            );

        await transaction.CommitAsync();

        return archivedStudent;
    }

    public static async ValueTask TryArchiveStudentIfHisDebtIsClosed(
        StudentEntity student,
        ApplicationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(student.Group);

        if (!StudentRequiresArchiving(student))
        {
            return;
        }

        var archiver = new StudentArchiver(context);

        var archivePayload = new InternalArchiveStudentPayload
        {
            ActiveSemesterName = (await context.GetActiveSemester()).Name,
            Visits = student.Visits,
            CurrentSemesterName = student.CurrentSemesterName,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            StudentGuid = student.StudentGuid,
            TotalPoints = CalculateTotalPoints(
                student.Visits,
                student.Group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ),
        };

        await archiver.ArchiveStudentAsync(archivePayload);
    }

    private static bool StudentRequiresArchiving(StudentEntity student)
    {
        ArgumentNullException.ThrowIfNull(student.Group);

        if (!student.HasDebtFromPreviousSemester)
        {
            return false;
        }

        return CalculateTotalPoints(
                student.Visits,
                student.Group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ) >= REQUIRED_POINT_AMOUNT;
    }

    private async Task ArchiveCurrentSemesterHistoryAsync(
        string studentGuid,
        string oldSemesterName
    )
    {
        await _applicationContext.VisitsStudentsHistory
            .Where(h => h.StudentGuid == studentGuid && h.IsArchived == true)
            .ExecuteDeleteAsync();

        await _applicationContext.PointsStudentsHistory
            .Where(h => h.StudentGuid == studentGuid && h.SemesterName == oldSemesterName)
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsArchived, true));

        await _applicationContext.VisitsStudentsHistory
            .Where(h => h.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsArchived, true));

        await _applicationContext.StandardsStudentsHistory
            .Where(h => h.StudentGuid == studentGuid && h.SemesterName == oldSemesterName)
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsArchived, true));
    }
}
