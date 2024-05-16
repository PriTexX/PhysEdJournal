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
        var student = await _applicationContext
            .Students.Where(s => s.StudentGuid == commandPayload.StudentGuid)
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

        var activeSemester = await _applicationContext.Semesters.SingleAsync(s => s.IsCurrent);

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

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            SemesterName = student.CurrentSemesterName,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            Visits = student.VisitsStudentHistory?.Count ?? 0,
            VisitsHistory =
                student
                    .VisitsStudentHistory?.Select(h => new ArchivedHistory
                    {
                        Date = h.Date,
                        StudentGuid = h.StudentGuid,
                        TeacherGuid = h.TeacherGuid,
                        Points = visitValue,
                    })
                    .ToList() ?? new List<ArchivedHistory>(),
            PointsHistory =
                student
                    .PointsStudentHistory?.Select(h => new ArchivedPointsHistory
                    {
                        Date = h.Date,
                        StudentGuid = h.StudentGuid,
                        TeacherGuid = h.TeacherGuid,
                        Points = h.Points,
                        WorkType = h.WorkType,
                        Comment = h.Comment,
                    })
                    .ToList() ?? new List<ArchivedPointsHistory>(),
            StandardsHistory =
                student
                    .StandardsStudentHistory?.Select(h => new ArchivedStandardsHistory
                    {
                        Date = h.Date,
                        StudentGuid = h.StudentGuid,
                        TeacherGuid = h.TeacherGuid,
                        Points = h.Points,
                        StandardType = h.StandardType,
                        Comment = h.Comment,
                    })
                    .ToList() ?? new List<ArchivedStandardsHistory>(),
        };

        _applicationContext.ArchivedStudents.Add(archivedStudent);

        student.VisitsStudentHistory?.Clear();
        student.PointsStudentHistory?.Clear();
        student.StandardsStudentHistory?.Clear();

        if (!student.HasDebtFromPreviousSemester)
        {
            student.HadDebtInSemester = false;
        }

        student.Visits = 0;
        student.AdditionalPoints = 0;
        student.PointsForStandards = 0;
        student.CurrentSemesterName = activeSemester.Name;
        student.ArchivedVisitValue = 0;
        student.HasDebtFromPreviousSemester = false;

        _applicationContext.Update(student);
        await _applicationContext.SaveChangesAsync();

        return archivedStudent;
    }
}
