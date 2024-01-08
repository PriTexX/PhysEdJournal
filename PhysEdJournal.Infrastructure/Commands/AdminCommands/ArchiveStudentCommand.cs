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
}

file struct HistoryEntity
{
    public required int Id { get; init; }
    public required DateOnly Date { get; init; }
    public required HistoryType Type { get; init; }
    public required double Points { get; init; }
}

file enum HistoryType
{
    AdditionalPoints,
    Visits,
    Standards,
}

public sealed class ArchiveStudentCommand : ICommand<ArchiveStudentCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public ArchiveStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(ArchiveStudentCommandPayload commandPayload)
    {
        var student = await _applicationContext.Students
            .Where(s => s.StudentGuid == commandPayload.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundException(commandPayload.StudentGuid);
        }

        ArgumentNullException.ThrowIfNull(student.Group);

        var visitValue = student.HasDebtFromPreviousSemester
            ? student.ArchivedVisitValue
            : student.Group.VisitValue;

        var totalPoints = CalculateTotalPoints(
            student.Visits,
            visitValue,
            student.AdditionalPoints,
            student.PointsForStandards
        );

        if (totalPoints < REQUIRED_POINT_AMOUNT)
        {
            if (!student.HasDebtFromPreviousSemester)
            {
                student.ArchivedVisitValue = student.Group.VisitValue;
                student.HasDebtFromPreviousSemester = true;

                _applicationContext.Update(student);
                await _applicationContext.SaveChangesAsync();
            }

            return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
        }

        var (visitsToArchive, standardsToArchive, pointsToArchive) = GetHistoriesToArchive(
            student.HasDebtFromPreviousSemester,
            visitValue,
            student.PointsStudentHistory,
            student.StandardsStudentHistory,
            student.VisitsStudentHistory
        );

        _applicationContext.ArchivedStudents.Add(
            new ArchivedStudentEntity
            {
                StudentGuid = student.StudentGuid,
                SemesterName = student.CurrentSemesterName,
                FullName = student.FullName,
                GroupNumber = student.GroupNumber,
                Visits = visitsToArchive.Count,
                VisitsHistory = visitsToArchive
                    .Select(
                        h =>
                            new ArchivedHistory
                            {
                                Date = h.Date,
                                StudentGuid = h.StudentGuid,
                                TeacherGuid = h.TeacherGuid,
                                Points = visitValue,
                            }
                    )
                    .ToList(),
                PointsHistory = pointsToArchive
                    .Select(
                        h =>
                            new ArchivedPointsHistory
                            {
                                Date = h.Date,
                                StudentGuid = h.StudentGuid,
                                TeacherGuid = h.TeacherGuid,
                                Points = h.Points,
                                WorkType = h.WorkType,
                                Comment = h.Comment,
                            }
                    )
                    .ToList(),
                StandardsHistory = standardsToArchive
                    .Select(
                        h =>
                            new ArchivedStandardsHistory
                            {
                                Date = h.Date,
                                StudentGuid = h.StudentGuid,
                                TeacherGuid = h.TeacherGuid,
                                Points = h.Points,
                                StandardType = h.StandardType,
                                Comment = h.Comment,
                            }
                    )
                    .ToList(),
            }
        );

        RemoveRange(student.VisitsStudentHistory, visitsToArchive);
        RemoveRange(student.PointsStudentHistory, pointsToArchive);
        RemoveRange(student.StandardsStudentHistory, standardsToArchive);

        var activeSemester = await _applicationContext.GetActiveSemester();

        student.Visits = student.VisitsStudentHistory?.Count ?? 0;
        student.AdditionalPoints = student.PointsStudentHistory?.Sum(h => h.Points) ?? 0;
        student.PointsForStandards = student.StandardsStudentHistory?.Sum(h => h.Points) ?? 0;
        student.CurrentSemesterName = activeSemester.Name;
        student.HasDebtFromPreviousSemester = false;

        _applicationContext.Update(student);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }

    private static void RemoveRange<T>(ICollection<T>? source, IEnumerable<T> itemsToDelete)
    {
        if (source is null)
        {
            return;
        }

        foreach (var record in itemsToDelete)
        {
            source.Remove(record);
        }
    }

    private static (
        List<VisitStudentHistoryEntity>,
        List<StandardsStudentHistoryEntity>,
        List<PointsStudentHistoryEntity>
    ) GetHistoriesToArchive(
        bool hasDebt,
        double visitValue,
        ICollection<PointsStudentHistoryEntity>? pointsHistory,
        ICollection<StandardsStudentHistoryEntity>? standardsHistory,
        ICollection<VisitStudentHistoryEntity>? visitsHistory
    )
    {
        if (!hasDebt)
        {
            var visits = visitsHistory?.ToList() ?? new List<VisitStudentHistoryEntity>();
            var standards = standardsHistory?.ToList() ?? new List<StandardsStudentHistoryEntity>();
            var points = pointsHistory?.ToList() ?? new List<PointsStudentHistoryEntity>();

            return (visits, standards, points);
        }

        var allHistory = new List<HistoryEntity>();

        if (pointsHistory is not null)
        {
            allHistory.AddRange(
                pointsHistory.Select(
                    h =>
                        new HistoryEntity
                        {
                            Id = h.Id,
                            Date = h.Date,
                            Type = HistoryType.AdditionalPoints,
                            Points = h.Points,
                        }
                )
            );
        }

        if (visitsHistory is not null)
        {
            allHistory.AddRange(
                visitsHistory.Select(
                    r =>
                        new HistoryEntity
                        {
                            Id = r.Id,
                            Date = r.Date,
                            Type = HistoryType.Visits,
                            Points = visitValue,
                        }
                )
            );
        }

        if (standardsHistory is not null)
        {
            allHistory.AddRange(
                standardsHistory.Select(
                    r =>
                        new HistoryEntity
                        {
                            Id = r.Id,
                            Date = r.Date,
                            Type = HistoryType.Standards,
                            Points = r.Points,
                        }
                )
            );
        }

        var sum = 0.0;
        var historiesToArchive = allHistory
            .OrderBy(r => r.Date)
            .TakeWhile(record =>
            {
                var needsToBeArchived = sum < REQUIRED_POINT_AMOUNT;
                sum += record.Points;
                return needsToBeArchived;
            })
            .ToList();

        var visitsToArchive =
            visitsHistory
                ?.Where(
                    h => historiesToArchive.Any(s => s.Id == h.Id && s.Type == HistoryType.Visits)
                )
                .ToList() ?? new List<VisitStudentHistoryEntity>();
        var pointsToArchive =
            pointsHistory
                ?.Where(
                    h =>
                        historiesToArchive.Any(
                            s => s.Id == h.Id && s.Type == HistoryType.AdditionalPoints
                        )
                )
                .ToList() ?? new List<PointsStudentHistoryEntity>();
        var standardsToArchive =
            standardsHistory
                ?.Where(
                    h =>
                        historiesToArchive.Any(s => s.Id == h.Id && s.Type == HistoryType.Standards)
                )
                .ToList() ?? new List<StandardsStudentHistoryEntity>();

        return (visitsToArchive, standardsToArchive, pointsToArchive);
    }
}
