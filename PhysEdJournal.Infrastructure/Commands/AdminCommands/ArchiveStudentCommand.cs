using HotChocolate.Language;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
using PhysEdJournal.Infrastructure.Services;
using PResult;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class ArchiveStudentCommandPayload
{
    public required string StudentGuid { get; init; }
    public required bool IsForceMode { get; init; } = false;
}

public sealed class ArchiveStudentCommand
    : ICommand<ArchiveStudentCommandPayload, ArchivedStudentEntity>
{
    private readonly ApplicationContext _applicationContext;
    private readonly StudentArchiver _studentArchiver;

    public ArchiveStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _studentArchiver = new StudentArchiver(applicationContext); // Деталь имплементации, поэтому не внедряю через DI
    }

    public async Task<Result<ArchivedStudentEntity>> ExecuteAsync(
        ArchiveStudentCommandPayload commandPayload
    )
    {
        var studentFromDb = await _applicationContext.Students
            .AsNoTracking()
            .Where(s => s.StudentGuid == commandPayload.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (studentFromDb is null)
        {
            return new StudentNotFoundException(commandPayload.StudentGuid);
        }

        if (studentFromDb.Group is null)
        {
            return new GroupNotFoundException(studentFromDb.GroupNumber);
        }

        var activeSemesterName = (await _applicationContext.GetActiveSemester()).Name;

        if (studentFromDb.CurrentSemesterName == activeSemesterName)
        {
            return new CannotMigrateToNewSemesterException(activeSemesterName);
        }

        var totalPoints = CalculateTotalPoints(
            studentFromDb.Visits,
            studentFromDb.Group.VisitValue,
            studentFromDb.AdditionalPoints,
            studentFromDb.PointsForStandards
        );

        if (studentFromDb.HasDebtFromPreviousSemester)
        {
            MarkHistoryToSave(
                totalPoints,
                studentFromDb.ArchivedVisitValue,
                studentFromDb.PointsStudentHistory,
                studentFromDb.StandardsStudentHistory,
                studentFromDb.VisitsStudentHistory
            );
        }

        // If student has enough points or he is force-archived
        if (commandPayload.IsForceMode || totalPoints >= REQUIRED_POINT_AMOUNT)
        {
            var archiveStudentPayload = new InternalArchiveStudentPayload
            {
                Visits = studentFromDb.Visits,
                FullName = studentFromDb.FullName,
                GroupNumber = studentFromDb.GroupNumber,
                StudentGuid = commandPayload.StudentGuid,
                TotalPoints = totalPoints,
                ActiveSemesterName = activeSemesterName,
                CurrentSemesterName = studentFromDb.CurrentSemesterName,
                HasDebt = false,
                VisitStudentHistory = studentFromDb.VisitsStudentHistory,
                PointsStudentHistory = studentFromDb.PointsStudentHistory,
                StandardsStudentHistory = studentFromDb.StandardsStudentHistory,
            };

            var archivedStudent = await _studentArchiver.ArchiveStudentAsync(archiveStudentPayload);
            return archivedStudent;
        }

        // If student is not force-archived and does not have enough points
        await _applicationContext.Students
            .Where(s => s.StudentGuid == commandPayload.StudentGuid)
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(s => s.HasDebtFromPreviousSemester, true)
                        .SetProperty(s => s.ArchivedVisitValue, studentFromDb.Group.VisitValue)
            );

        return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
    }

    private void MarkHistoryToSave(
        double totalPoints,
        double oldVisitValue,
        ICollection<PointsStudentHistoryEntity>? pointsHistory,
        ICollection<StandardsStudentHistoryEntity>? standardsHistory,
        ICollection<VisitStudentHistoryEntity>? visitsHistory
    )
    {
        if (totalPoints <= REQUIRED_POINT_AMOUNT)
        {
            return;
        }

        if (pointsHistory is null && standardsHistory is null && visitsHistory is null)
        {
            throw new Exception("Has total points with no history");
        }

        var allHistory = new List<SuperHistoryEntity>();

        if (pointsHistory is not null)
        {
            allHistory = pointsHistory
                .Select(
                    r =>
                        new SuperHistoryEntity
                        {
                            Id = r.Id,
                            Date = r.Date,
                            HistoryType = nameof(r),
                            Points = r.Points,
                        }
                )
                .ToList();
        }

        if (visitsHistory is not null)
        {
            allHistory.AddRange(
                visitsHistory.Select(
                    r =>
                        new SuperHistoryEntity
                        {
                            Id = r.Id,
                            Date = r.Date,
                            HistoryType = nameof(r),
                            Points = oldVisitValue,
                        }
                )
            );
        }

        if (standardsHistory is not null)
        {
            allHistory.AddRange(
                standardsHistory.Select(
                    r =>
                        new SuperHistoryEntity
                        {
                            Id = r.Id,
                            Date = r.Date,
                            HistoryType = nameof(r),
                            Points = r.Points,
                        }
                )
            );
        }

        allHistory = allHistory.OrderByDescending(r => r.Date).ToList();

        double sum = 0;
        do
        {
            var record = allHistory.Pop();
            sum += record.Points;
        } while (sum < REQUIRED_POINT_AMOUNT);

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(VisitStudentHistoryEntity))
            {
                continue;
            }

            var record = visitsHistory?.FirstOrDefault(h => h.Id == r.Id);

            if (record is null)
            {
                continue;
            }

            record.ShouldBeArchived = false;
        }

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(PointsStudentHistoryEntity))
            {
                continue;
            }

            var record = pointsHistory?.First(h => h.Id == r.Id);

            if (record is null)
            {
                continue;
            }

            record.ShouldBeArchived = false;
        }

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(StandardsStudentHistoryEntity))
            {
                continue;
            }

            var record = standardsHistory?.First(h => h.Id == r.Id);

            if (record is null)
            {
                continue;
            }

            record.ShouldBeArchived = false;
        }
    }

    private class SuperHistoryEntity
    {
        public required int Id { get; init; }
        public required DateOnly Date { get; init; }
        public required string HistoryType { get; init; }
        public required double Points { get; init; }
    }
}
