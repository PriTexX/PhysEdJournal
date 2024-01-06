using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
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

    public ArchiveStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
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

        ArgumentNullException.ThrowIfNull(studentFromDb.Group);

        studentFromDb.StandardsStudentHistory ??= new List<StandardsStudentHistoryEntity>();
        studentFromDb.VisitsStudentHistory ??= new List<VisitStudentHistoryEntity>();
        studentFromDb.PointsStudentHistory ??= new List<PointsStudentHistoryEntity>();

        var activeSemesterName = (await _applicationContext.GetActiveSemester()).Name;

        var totalPoints = CalculateTotalPoints(
            studentFromDb.Visits,
            studentFromDb.Group.VisitValue,
            studentFromDb.AdditionalPoints,
            studentFromDb.PointsForStandards
        );

        if (!commandPayload.IsForceMode && totalPoints < REQUIRED_POINT_AMOUNT)
        {
            studentFromDb.ArchivedVisitValue = studentFromDb.Group.VisitValue;
            studentFromDb.HasDebtFromPreviousSemester = true;

            _applicationContext.Students.Update(studentFromDb);
            await _applicationContext.SaveChangesAsync();

            return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
        }

        var histories = FindHistoriesToArchive(
                totalPoints,
                studentFromDb.ArchivedVisitValue,
                studentFromDb.PointsStudentHistory,
                studentFromDb.StandardsStudentHistory,
                studentFromDb.VisitsStudentHistory
            )
            .ToList();

        var visitsToArchive = studentFromDb.VisitsStudentHistory
            .Where(h => histories.Any(s => s.Id == h.Id && s.Type == HistoryType.VisitsHistory))
            .ToList();
        var pointsToArchive = studentFromDb.PointsStudentHistory
            .Where(h => histories.Any(s => s.Id == h.Id && s.Type == HistoryType.PointsHistory))
            .ToList();
        var standardsToArchive = studentFromDb.StandardsStudentHistory
            .Where(h => histories.Any(s => s.Id == h.Id && s.Type == HistoryType.StandardsHistory))
            .ToList();

        RemoveRange(studentFromDb.VisitsStudentHistory, visitsToArchive);

        RemoveRange(studentFromDb.PointsStudentHistory, pointsToArchive);

        RemoveRange(studentFromDb.StandardsStudentHistory, standardsToArchive);

        var archiveStudentPayload = new ArchiveStudentPayload
        {
            Visits = studentFromDb.Visits,
            FullName = studentFromDb.FullName,
            GroupNumber = studentFromDb.GroupNumber,
            StudentGuid = commandPayload.StudentGuid,
            TotalPoints = totalPoints,
            ActiveSemesterName = activeSemesterName,
            CurrentSemesterName = studentFromDb.CurrentSemesterName,
            HasDebt = false,
            VisitStudentHistory = visitsToArchive,
            PointsStudentHistory = pointsToArchive,
            StandardsStudentHistory = standardsToArchive,
        };

        var archivedStudent = await ArchiveStudentAsync(archiveStudentPayload);

        studentFromDb.Visits = studentFromDb.VisitsStudentHistory?.Count ?? 0;
        studentFromDb.AdditionalPoints =
            studentFromDb.PointsStudentHistory?.Sum(h => h.Points) ?? 0;
        studentFromDb.PointsForStandards =
            studentFromDb.StandardsStudentHistory?.Sum(h => h.Points) ?? 0;
        studentFromDb.HasDebtFromPreviousSemester = false;
        studentFromDb.AdditionalPoints = 0;

        _applicationContext.Students.Update(studentFromDb);
        await _applicationContext.SaveChangesAsync();
        return archivedStudent;
    }

    private void RemoveRange<T>(ICollection<T> source, IEnumerable<T> itemsToDelete)
    {
        foreach (var record in itemsToDelete)
        {
            source.Remove(record);
        }
    }

    private IEnumerable<SuperHistoryEntity> FindHistoriesToArchive(
        double totalPoints,
        double oldVisitValue,
        IEnumerable<PointsStudentHistoryEntity> pointsHistory,
        IEnumerable<StandardsStudentHistoryEntity> standardsHistory,
        IEnumerable<VisitStudentHistoryEntity> visitsHistory
    )
    {
        var allHistory = pointsHistory
            .Select(
                r =>
                    new SuperHistoryEntity
                    {
                        Id = r.Id,
                        Date = r.Date,
                        Type = HistoryType.PointsHistory,
                        Points = r.Points,
                    }
            )
            .ToList();

        allHistory.AddRange(
            visitsHistory.Select(
                r =>
                    new SuperHistoryEntity
                    {
                        Id = r.Id,
                        Date = r.Date,
                        Type = HistoryType.VisitsHistory,
                        Points = oldVisitValue,
                    }
            )
        );

        allHistory.AddRange(
            standardsHistory.Select(
                r =>
                    new SuperHistoryEntity
                    {
                        Id = r.Id,
                        Date = r.Date,
                        Type = HistoryType.StandardsHistory,
                        Points = r.Points,
                    }
            )
        );

        if (totalPoints > REQUIRED_POINT_AMOUNT)
        {
            var sum = 0.0;
            var historyToArchive = allHistory
                .OrderByDescending(r => r.Date)
                .TakeWhile(record =>
                {
                    sum += record.Points;
                    return sum < REQUIRED_POINT_AMOUNT;
                })
                .ToList();

            return historyToArchive;
        }

        return allHistory;
    }

    private async Task<ArchivedStudentEntity> ArchiveStudentAsync(ArchiveStudentPayload student)
    {
        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = student.TotalPoints,
            Visits = student.Visits,
            SemesterName = student.CurrentSemesterName,
            VisitStudentHistory = student.VisitStudentHistory,
            PointsStudentHistory = student.PointsStudentHistory,
            StandardsStudentHistory = student.StandardsStudentHistory,
        };

        await _applicationContext.ArchivedStudents.AddAsync(archivedStudent);

        return archivedStudent;
    }

    private struct SuperHistoryEntity
    {
        public required int Id { get; init; }
        public required DateOnly Date { get; init; }
        public required HistoryType Type { get; init; }
        public required double Points { get; init; }
    }

    private enum HistoryType
    {
        PointsHistory,
        VisitsHistory,
        StandardsHistory,
    }

    private sealed class ArchiveStudentPayload
    {
        public required string StudentGuid { get; init; }
        public required int Visits { get; init; }
        public required string FullName { get; init; }
        public required string GroupNumber { get; init; }
        public required string CurrentSemesterName { get; init; }
        public required string ActiveSemesterName { get; init; }
        public required double TotalPoints { get; init; }

        public required bool HasDebt { get; init; }

        public required ICollection<VisitStudentHistoryEntity> VisitStudentHistory { get; init; }

        public required ICollection<PointsStudentHistoryEntity> PointsStudentHistory { get; init; }

        public required ICollection<StandardsStudentHistoryEntity> StandardsStudentHistory { get; init; }
    }
}
