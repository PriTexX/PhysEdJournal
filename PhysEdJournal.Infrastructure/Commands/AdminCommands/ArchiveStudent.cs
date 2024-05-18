using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class ArchiveStudentPayload
{
    public required string StudentGuid { get; init; }
    public string? TeacherGuid { get; init; }
}

file sealed class ArchiveStudentCommandValidator : ICommandValidator<ArchiveStudentPayload>
{
    private readonly ApplicationContext _applicationContext;

    public ArchiveStudentCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(ArchiveStudentPayload input)
    {
        var student = await _applicationContext
            .Students.Where(s => s.StudentGuid == input.StudentGuid)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundException(input.StudentGuid);
        }

        ArgumentNullException.ThrowIfNull(student.Group);

        var semester = await _applicationContext.Semesters.SingleAsync(s => s.IsCurrent);

        if (student.CurrentSemesterName == semester.Name)
        {
            return new SemesterMismatchError();
        }

        if (input.TeacherGuid is not null && student.Group.CuratorGuid != input.TeacherGuid)
        {
            return new NotCuratorError();
        }

        return ValidationResult.Success;
    }
}

public sealed class ArchiveStudentCommand : ICommand<ArchiveStudentPayload, ArchivedStudentEntity>
{
    private readonly ApplicationContext _applicationContext;
    private readonly ICommandValidator<ArchiveStudentPayload> _validator;

    public ArchiveStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new ArchiveStudentCommandValidator(applicationContext);
    }

    public async Task<Result<ArchivedStudentEntity>> ExecuteAsync(ArchiveStudentPayload payload)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(payload);

        if (validationResult.IsFailed)
        {
            return validationResult.ValidationException;
        }

        var student = await _applicationContext
            .Students.Where(s => s.StudentGuid == payload.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundException(payload.StudentGuid);
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

            return new NotEnoughPointsException(payload.StudentGuid, totalPoints);
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
