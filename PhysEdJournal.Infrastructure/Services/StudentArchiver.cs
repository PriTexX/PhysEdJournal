using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;

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
