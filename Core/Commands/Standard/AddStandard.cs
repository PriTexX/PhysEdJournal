using Core.Config;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class AddStandardPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required StandardType StandardType { get; init; }
    public required bool IsAdminOrSecretary { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddStandardValidator : ICommandValidator<AddStandardPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddStandardValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddStandardPayload payload)
    {
        if (payload.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureError();
        }

        var student = await _applicationContext
            .Students.Include(s => s.Group)
            .Where(s => s.StudentGuid == payload.StudentGuid)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        ArgumentNullException.ThrowIfNull(student.Group);

        var visitValue = student.HasDebt ? student.ArchivedVisitValue : student.Group.VisitValue;

        var totalPoints = Cfg.CalculateTotalPoints(
            student.Visits,
            visitValue,
            student.AdditionalPoints,
            student.PointsForStandards
        );

        if (
            totalPoints < Cfg.MinTotalPointsToAddStandards
            || (
                student.Course > 1
                && totalPoints < Cfg.MinTotalPointsToAddStandardsForCoursesHigherThan1
            )
        )
        {
            return new NotEnoughPointsForStandardsError();
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - payload.Date.DayNumber
                > Cfg.VisitAndStandardsLifeDays
            && !payload.IsAdminOrSecretary
        )
        {
            return new DateExpiredError();
        }

        if (payload.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayError();
        }

        if (
            payload.Points > Cfg.MaxPointsForOneStandard
            || (
                student.Course > 1
                && payload.Points > Cfg.MaxPointsForOneStandardForCoursesHigherThan1
            )
        )
        {
            return new PointsOutOfLimitError();
        }

        var duplicateHistoryEntity = await _applicationContext
            .StandardsStudentsHistory.AsNoTracking()
            .Where(s =>
                s.StudentGuid == payload.StudentGuid && s.StandardType == payload.StandardType
            )
            .OrderByDescending(s => s.Points)
            .FirstOrDefaultAsync();

        if (duplicateHistoryEntity is not null && payload.StandardType != StandardType.Other)
        {
            return new StandardExistsError();
        }

        return ValidationResult.Success;
    }
}

public sealed class AddStandardCommand : ICommand<AddStandardPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddStandardValidator _validator;

    public AddStandardCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddStandardValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddStandardPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await _applicationContext
            .Students.Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.StudentGuid == commandPayload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        var standardsStudentHistoryEntity = new StandardsHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            TeacherGuid = commandPayload.TeacherGuid,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            StandardType = commandPayload.StandardType,
            Comment = commandPayload.Comment,
        };

        var adjustedStudentPointsAmount = AdjustStudentPointsAmount(
            student.PointsForStandards,
            standardsStudentHistoryEntity.Points
        );

        student.PointsForStandards = adjustedStudentPointsAmount;

        _applicationContext.StandardsStudentsHistory.Add(standardsStudentHistoryEntity);
        _applicationContext.Students.Update(student);

        try
        {
            await _applicationContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConcurrencyError();
        }

        return Unit.Default;
    }

    private int AdjustStudentPointsAmount(int studentTotalPointsForStandards, int pointsToImplement)
    {
        int adjustedStudentPointsAmount;
        if (studentTotalPointsForStandards + pointsToImplement > Cfg.MaxPointsForStandards)
        {
            adjustedStudentPointsAmount = Cfg.MaxPointsForStandards;
        }
        else
        {
            adjustedStudentPointsAmount = studentTotalPointsForStandards + pointsToImplement;
        }

        return adjustedStudentPointsAmount;
    }
}
