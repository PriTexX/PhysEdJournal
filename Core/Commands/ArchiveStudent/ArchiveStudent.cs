using Core.Config;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class ArchiveStudentPayload
{
    public required string StudentGuid { get; init; }
    public string? TeacherGuid { get; init; }
    public required bool IsAdmin { get; init; }
}

file sealed class ArchiveStudentValidator : ICommandValidator<ArchiveStudentPayload>
{
    private readonly ApplicationContext _applicationContext;

    public ArchiveStudentValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(ArchiveStudentPayload input)
    {
        var student = await this
            ._applicationContext.Students.Where(s => s.StudentGuid == input.StudentGuid)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        ArgumentNullException.ThrowIfNull(student.Group);

        var semester = await _applicationContext.Semesters.SingleAsync(s => s.IsCurrent);

        if (student.CurrentSemesterName == semester.Name)
        {
            return new SameSemesterError();
        }

        if (
            input.TeacherGuid is not null
            && !input.IsAdmin
            && student.Group.CuratorGuid != input.TeacherGuid
        )
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
        _validator = new ArchiveStudentValidator(applicationContext);
    }

    public async Task<Result<ArchivedStudentEntity>> ExecuteAsync(ArchiveStudentPayload payload)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(payload);

        if (validationResult.IsFailed)
        {
            return validationResult.ValidationException;
        }

        var student = await this
            ._applicationContext.Students.Where(s => s.StudentGuid == payload.StudentGuid)
            .Include(s => s.PointsHistory)
            .Include(s => s.StandardsHistory)
            .Include(s => s.VisitsHistory)
            .Include(s => s.Group)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        ArgumentNullException.ThrowIfNull(student.Group);

        var activeSemester = await _applicationContext.Semesters.SingleAsync(s => s.IsCurrent);

        var visitValue = student.HasDebt ? student.ArchivedVisitValue : student.Group.VisitValue;

        var totalPoints = Cfg.CalculateTotalPoints(
            student.Visits,
            visitValue,
            student.AdditionalPoints,
            student.PointsForStandards
        );

        if (totalPoints < Cfg.RequiredPointsAmount)
        {
            if (student.HasDebt)
            {
                return new NotEnoughPointsError();
            }

            student.ArchivedVisitValue = student.Group.VisitValue;
            student.HasDebt = true;
            student.HadDebtInSemester = true;

            _applicationContext.Update(student);
            await _applicationContext.SaveChangesAsync();

            return new NotEnoughPointsError();
        }

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            SemesterName = student.CurrentSemesterName,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            Visits = student.VisitsHistory?.Count ?? 0,
            TotalPoints = totalPoints,
            VisitsHistory =
                student
                    .VisitsHistory?.Select(h => new ArchivedHistory
                    {
                        Date = h.Date,
                        StudentGuid = h.StudentGuid,
                        TeacherGuid = h.TeacherGuid,
                        Points = visitValue,
                    })
                    .ToList() ?? new List<ArchivedHistory>(),
            PointsHistory =
                student
                    .PointsHistory?.Select(h => new ArchivedPointsHistory
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
                    .StandardsHistory?.Select(h => new ArchivedStandardsHistory
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

        student.VisitsHistory?.Clear();
        student.PointsHistory?.Clear();
        student.StandardsHistory?.Clear();

        if (!student.HasDebt)
        {
            student.HadDebtInSemester = false;
        }

        student.Visits = 0;
        student.AdditionalPoints = 0;
        student.PointsForStandards = 0;
        student.CurrentSemesterName = activeSemester.Name;
        student.ArchivedVisitValue = 0;
        student.HasDebt = false;

        _applicationContext.Update(student);
        await _applicationContext.SaveChangesAsync();

        return archivedStudent;
    }
}
