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

        var visitsToArchive = student.VisitStudentHistory?.Where(e => e.ShouldBeArchived).ToList();
        var pointsToArchive = student.PointsStudentHistory?.Where(e => e.ShouldBeArchived).ToList();
        var standardsToArchive = student.StandardsStudentHistory
            ?.Where(e => e.ShouldBeArchived)
            .ToList();

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = student.TotalPoints,
            Visits = student.Visits,
            SemesterName = student.CurrentSemesterName,
            VisitStudentHistory = visitsToArchive,
            PointsStudentHistory = pointsToArchive,
            StandardsStudentHistory = standardsToArchive,
        };

        _applicationContext.ArchivedStudents.Add(archivedStudent);
        await _applicationContext.SaveChangesAsync();

        var visitsToSave = student.VisitStudentHistory?.Where(e => !e.ShouldBeArchived).ToList();
        var pointsToSave = student.PointsStudentHistory?.Where(e => !e.ShouldBeArchived).ToList();
        var standardsToSave = student.StandardsStudentHistory
            ?.Where(e => !e.ShouldBeArchived)
            .ToList();

        var visits = visitsToSave?.Count ?? 0;
        var additionalPoints = pointsToArchive?.Sum(r => r.Points) ?? 0;
        var pointsForStandards = standardsToArchive?.Sum(r => r.Points) ?? 0;

        await _applicationContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(s => s.AdditionalPoints, additionalPoints)
                        .SetProperty(s => s.Visits, visits)
                        .SetProperty(s => s.PointsForStandards, pointsForStandards)
                        .SetProperty(s => s.HasDebtFromPreviousSemester, false)
                        .SetProperty(s => s.ArchivedVisitValue, 0)
            );

        if (visitsToSave is not null)
        {
            await _applicationContext.VisitsStudentsHistory.AddRangeAsync(visitsToSave);
        }

        if (pointsToSave is not null)
        {
            await _applicationContext.PointsStudentsHistory.AddRangeAsync(pointsToSave);
        }

        if (standardsToSave is not null)
        {
            await _applicationContext.StandardsStudentsHistory.AddRangeAsync(standardsToSave);
        }

        await transaction.CommitAsync();

        return archivedStudent;
    }
}
