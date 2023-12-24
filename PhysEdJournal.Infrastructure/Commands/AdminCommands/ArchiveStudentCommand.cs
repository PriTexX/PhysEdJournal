using HotChocolate.Language;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
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
            .Select(
                s =>
                    new
                    {
                        s.Group!.VisitValue,
                        s.Visits,
                        s.AdditionalPoints,
                        s.PointsForStandards,
                        s.FullName,
                        s.GroupNumber,
                        s.HasDebtFromPreviousSemester,
                        s.ArchivedVisitValue,
                        s.CurrentSemesterName,
                        s.PointsStudentHistory,
                        s.StandardsStudentHistory,
                        s.VisitsStudentHistory,
                    }
            )
            .FirstOrDefaultAsync();

        if (studentFromDb is null)
        {
            return new StudentNotFoundException(commandPayload.StudentGuid);
        }

        var activeSemesterName = (await _applicationContext.GetActiveSemester()).Name;

        if (studentFromDb.CurrentSemesterName == activeSemesterName)
        {
            return new CannotMigrateToNewSemesterException(activeSemesterName);
        }

        var totalPoints = CalculateTotalPoints(
            studentFromDb.Visits,
            studentFromDb.VisitValue,
            studentFromDb.AdditionalPoints,
            studentFromDb.PointsForStandards
        );

        if (studentFromDb.HasDebtFromPreviousSemester)
        {
            TrySavePointsFromCurrentSemester(
                totalPoints,
                oldVisitValue: 0, // TODO: узнать откуда брать
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
                        .SetProperty(s => s.ArchivedVisitValue, studentFromDb.VisitValue)
            );

        return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
    }

    private (
        ICollection<VisitStudentHistoryEntity>,
        ICollection<PointsStudentHistoryEntity>,
        ICollection<StandardsStudentHistoryEntity>
    ) TrySavePointsFromCurrentSemester(
        double totalPoints,
        double oldVisitValue,
        ICollection<PointsStudentHistoryEntity>? pointsHistory,
        ICollection<StandardsStudentHistoryEntity>? standardsHistory,
        ICollection<VisitStudentHistoryEntity>? visitsHistory
    )
    {
        if (pointsHistory is null && standardsHistory is null && visitsHistory is null)
        {
            return (
                new List<VisitStudentHistoryEntity>(),
                new List<PointsStudentHistoryEntity>(),
                new List<StandardsStudentHistoryEntity>()
            );
        }

        if (totalPoints <= REQUIRED_POINT_AMOUNT)
        {
            return (
                new List<VisitStudentHistoryEntity>(),
                new List<PointsStudentHistoryEntity>(),
                new List<StandardsStudentHistoryEntity>()
            );
        }

        var allHistory = new List<SuperHistoryEntity>();

        foreach (var r in pointsHistory)
        {
            allHistory.Add(
                new SuperHistoryEntity
                {
                    Id = r.Id,
                    Date = r.Date,
                    HistoryType = nameof(r),
                    Points = r.Points,
                }
            );
        }

        foreach (var r in visitsHistory)
        {
            allHistory.Add(
                new SuperHistoryEntity
                {
                    Id = r.Id,
                    Date = r.Date,
                    HistoryType = nameof(r),
                    Points = oldVisitValue,
                }
            );
        }

        foreach (var r in standardsHistory)
        {
            allHistory.Add(
                new SuperHistoryEntity
                {
                    Id = r.Id,
                    Date = r.Date,
                    HistoryType = nameof(r),
                    Points = r.Points,
                }
            );
        }

        allHistory = allHistory.OrderByDescending(r => r.Date).ToList();

        double sum = 0;
        do
        {
            var record = allHistory.Pop();
            sum += record.Points;
        } while (sum < REQUIRED_POINT_AMOUNT);

        var newVisits = new List<VisitStudentHistoryEntity>();
        var newPoints = new List<PointsStudentHistoryEntity>();
        var newStandards = new List<StandardsStudentHistoryEntity>();

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(VisitStudentHistoryEntity))
            {
                continue;
            }

            var record = visitsHistory.First(h => h.Id == r.Id);
            newVisits.Add(record);
            visitsHistory.Remove(record);
        }

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(PointsStudentHistoryEntity))
            {
                continue;
            }

            var record = pointsHistory.First(h => h.Id == r.Id);
            newPoints.Add(record);
            pointsHistory.Remove(record);
        }

        foreach (var r in allHistory)
        {
            if (r.HistoryType != nameof(StandardsStudentHistoryEntity))
            {
                continue;
            }

            var record = standardsHistory.First(h => h.Id == r.Id);
            newStandards.Add(record);
            standardsHistory.Remove(record);
        }

        return (newVisits, newPoints, newStandards);
    }

    private class SuperHistoryEntity
    {
        public required int Id { get; init; }
        public required DateOnly Date { get; init; }
        public required string HistoryType { get; init; }
        public required double Points { get; init; }
    }
}
