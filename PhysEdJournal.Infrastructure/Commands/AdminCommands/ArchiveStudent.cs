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
    public required string SemesterName { get; init; }
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
                student.HadDebtInSemester = true;

                _applicationContext.Update(student);
                await _applicationContext.SaveChangesAsync();
            }

            return new NotEnoughPointsException(commandPayload.StudentGuid, totalPoints);
        }

        var visitsToArchive = GetHistoriesToArchive(
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
                PointsHistory =
                    student.PointsStudentHistory
                        ?.Select(
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
                        .ToList() ?? new List<ArchivedPointsHistory>(),
                StandardsHistory =
                    student.StandardsStudentHistory
                        ?.Select(
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
                        .ToList() ?? new List<ArchivedStandardsHistory>(),
            }
        );

        if (!student.HasDebtFromPreviousSemester)
        {
            student.HadDebtInSemester = false;
        }

        visitsToArchive.ForEach(v => student.VisitsStudentHistory?.Remove(v));
        student.PointsStudentHistory?.Clear();
        student.StandardsStudentHistory?.Clear();

        student.Visits = student.VisitsStudentHistory?.Count ?? 0;
        student.AdditionalPoints = student.PointsStudentHistory?.Sum(h => h.Points) ?? 0;
        student.PointsForStandards = student.StandardsStudentHistory?.Sum(h => h.Points) ?? 0;
        student.CurrentSemesterName = commandPayload.SemesterName;
        student.ArchivedVisitValue = 0;
        student.HasDebtFromPreviousSemester = false;

        _applicationContext.Update(student);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }

    private static List<VisitStudentHistoryEntity> GetHistoriesToArchive(
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
            return visits;
        }

        var standardsPointsSum = standardsHistory?.Sum(h => h.Points) ?? 0.0;

        var additionalPointsSum = pointsHistory?.Sum(h => h.Points) ?? 0.0;

        if (standardsPointsSum > MAX_POINTS_FOR_STANDARDS)
        {
            standardsPointsSum = MAX_POINTS_FOR_STANDARDS;
        }

        var sum = standardsPointsSum + additionalPointsSum;

        var visitsToArchive = visitsHistory
            ?.OrderBy(r => r.Date)
            .TakeWhile(record =>
            {
                var needsToBeArchived = sum < REQUIRED_POINT_AMOUNT;
                sum += visitValue;
                return needsToBeArchived;
            })
            .ToList();

        return visitsToArchive ?? new List<VisitStudentHistoryEntity>();
    }
}
