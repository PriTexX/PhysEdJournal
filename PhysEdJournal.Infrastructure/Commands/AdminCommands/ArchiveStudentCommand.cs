using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;
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

        var visitsToArchive = studentFromDb.VisitsStudentHistory;
        var pointsToArchive = studentFromDb.PointsStudentHistory;
        var standardsToArchive = studentFromDb.StandardsStudentHistory;

        if (studentFromDb.HasDebtFromPreviousSemester && totalPoints > REQUIRED_POINT_AMOUNT)
        {
            var histories = FindHistoriesToArchive(
                studentFromDb.ArchivedVisitValue,
                studentFromDb.PointsStudentHistory,
                studentFromDb.StandardsStudentHistory,
                studentFromDb.VisitsStudentHistory
            );

            visitsToArchive = studentFromDb.VisitsStudentHistory.Filter(
                h => histories.Include(h.Id)
            );
            pointsToArchive = studentFromDb.PointsStudentHistory.Filter(
                h => histories.Include(h.Id)
            );
            standardsToArchive = studentFromDb.StandardsStudentHistory.Filter(
                h => histories.Include(h.Id)
            );

            foreach (var r in histories)
            {
                switch (r.Type)
                {
                    case HistoryType.PointsHistory:
                    {
                        var record = studentFromDb.PointsStudentHistory?.FirstOrDefault(
                            h => h.Id == r.Id
                        );

                        if (record is null)
                        {
                            return new Exception(
                                $"History of type {r.Type} or record with id = {r.Id} is missing"
                            );
                        }

                        pointsToArchive.Add(record);
                        studentFromDb.PointsStudentHistory?.Remove(record);
                        _applicationContext.PointsStudentsHistory.Remove(record);

                        continue;
                    }

                    case HistoryType.VisitsHistory:
                    {
                        var record = studentFromDb.VisitsStudentHistory?.FirstOrDefault(
                            h => h.Id == r.Id
                        );

                        if (record is null)
                        {
                            return new Exception(
                                $"History of type {r.Type} or record with id = {r.Id} is missing"
                            );
                        }

                        visitsToArchive.Add(record);
                        studentFromDb.VisitsStudentHistory?.Remove(record);
                        _applicationContext.VisitsStudentsHistory.Remove(record);

                        continue;
                    }

                    case HistoryType.StandardsHistory:
                    {
                        var record = studentFromDb.StandardsStudentHistory?.FirstOrDefault(
                            h => h.Id == r.Id
                        );

                        if (record is null)
                        {
                            return new Exception(
                                $"History of type {r.Type} or record with id = {r.Id} is missing"
                            );
                        }

                        standardsToArchive.Add(record);
                        studentFromDb.StandardsStudentHistory?.Remove(record);
                        _applicationContext.StandardsStudentsHistory.Remove(record);

                        continue;
                    }

                    default:
                        return new Exception($"History type {r.Type} undefined");
                }
            }
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
                VisitStudentHistory = visitsToArchive,
                PointsStudentHistory = pointsToArchive,
                StandardsStudentHistory = standardsToArchive,
            };

            var archivedStudent = await ArchiveStudentAsync(archiveStudentPayload);

            if (!studentFromDb.HasDebtFromPreviousSemester)
            {
                await DeleteAllHistoryNoSaveAsync(studentFromDb.StudentGuid);

                studentFromDb.StandardsStudentHistory?.Clear();
                studentFromDb.VisitsStudentHistory?.Clear();
                studentFromDb.PointsStudentHistory?.Clear();
            }

            studentFromDb.Visits = studentFromDb.VisitsStudentHistory?.Count ?? 0;
            studentFromDb.AdditionalPoints = studentFromDb.PointsStudentHistory?.Count ?? 0;
            studentFromDb.PointsForStandards = studentFromDb.StandardsStudentHistory?.Count ?? 0;
            studentFromDb.HasDebtFromPreviousSemester = false;
            studentFromDb.AdditionalPoints = 0;

            _applicationContext.Students.Update(studentFromDb);
            await _applicationContext.SaveChangesAsync();
            return archivedStudent;
        }

        studentFromDb.ArchivedVisitValue = studentFromDb.Group.VisitValue;
        studentFromDb.HasDebtFromPreviousSemester = true;

        _applicationContext.Students.Update(studentFromDb);
        await _applicationContext.SaveChangesAsync();

        return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
    }

    private async Task DeleteAllHistoryNoSaveAsync(string studentGuid)
    {
        var pointsHistory = await _applicationContext.PointsStudentsHistory
            .Where(r => r.StudentGuid == studentGuid)
            .ToListAsync();
        var standardsHistory = await _applicationContext.StandardsStudentsHistory
            .Where(r => r.StudentGuid == studentGuid)
            .ToListAsync();
        var visitsHistory = await _applicationContext.VisitsStudentsHistory
            .Where(r => r.StudentGuid == studentGuid)
            .ToListAsync();

        _applicationContext.PointsStudentsHistory.RemoveRange(pointsHistory);

        _applicationContext.StandardsStudentsHistory.RemoveRange(standardsHistory);

        _applicationContext.VisitsStudentsHistory.RemoveRange(visitsHistory);
    }

    private IEnumerable<SuperHistoryEntity> FindHistoriesToArchive(
        double oldVisitValue,
        ICollection<PointsStudentHistoryEntity>? pointsHistory,
        ICollection<StandardsStudentHistoryEntity>? standardsHistory,
        ICollection<VisitStudentHistoryEntity>? visitsHistory
    )
    {
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
                            Type = HistoryType.PointsHistory,
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
                            Type = HistoryType.VisitsHistory,
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
                            Type = HistoryType.StandardsHistory,
                            Points = r.Points,
                        }
                )
            );
        }

        allHistory = allHistory.OrderByDescending(r => r.Date).ToList();
        var historyToArchive = new List<SuperHistoryEntity>();

        double sum = 0;
        do
        {
            var record = allHistory.Pop();
            historyToArchive.Add(record);
            sum += record.Points;
        } while (sum < REQUIRED_POINT_AMOUNT);

        return historyToArchive;
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

    private async Task<ArchivedStudentEntity> ArchiveStudentAsync(
        InternalArchiveStudentPayload student
    )
    {
        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = student.TotalPoints,
            Visits = student.Visits,
            SemesterName = student.CurrentSemesterName,
        };

        if (student.VisitStudentHistory is not null)
        {
            archivedStudent.VisitStudentHistory = student.VisitStudentHistory;
        }

        if (student.PointsStudentHistory is not null)
        {
            archivedStudent.PointsStudentHistory = student.PointsStudentHistory;
        }

        if (student.StandardsStudentHistory is not null)
        {
            archivedStudent.StandardsStudentHistory = student.StandardsStudentHistory;
        }

        _applicationContext.ArchivedStudents.Add(archivedStudent);
        await _applicationContext.SaveChangesAsync();

        return archivedStudent;
    }
}
