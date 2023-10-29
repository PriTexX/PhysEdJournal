using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddStandardPointsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required StandardType StandardType { get; init; }
    public required bool IsOverride { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddStandardPointsCommandValidator
    : ICommandValidator<AddStandardPointsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddStandardPointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        AddStandardPointsCommandPayload commandInput
    )
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureException(commandInput.Date);
        }

        var student = await _applicationContext.Students
            .Include(s => s.Group)
            .Where(s => s.StudentGuid == commandInput.StudentGuid)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return new StudentNotFoundException(commandInput.StudentGuid);
        }

        var totalPoints = CalculateTotalPoints(
            student.Visits,
            student.Group!.VisitValue,
            student.AdditionalPoints,
            student.PointsForStandards
        );

        if (totalPoints < 20)
        {
            return new NotEnoughPointsForStandardException();
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - commandInput.Date.DayNumber
            > POINTS_LIFE_DAYS
        )
        {
            return new DateExpiredException(commandInput.Date);
        }

        if (commandInput.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayException(commandInput.Date.DayOfWeek);
        }

        if (commandInput.Points > MAX_POINTS_FOR_ONE_STANDARD)
        {
            return new PointsOverflowException(commandInput.Points, MAX_POINTS_FOR_ONE_STANDARD);
        }

        if (commandInput.Points <= 0)
        {
            return new NegativePointAmount();
        }

        var duplicateHistoryEntity = await _applicationContext.StandardsStudentsHistory
            .AsNoTracking()
            .Where(
                s =>
                    s.StudentGuid == commandInput.StudentGuid
                    && s.StandardType == commandInput.StandardType
            )
            .OrderByDescending(s => s.Points)
            .FirstOrDefaultAsync();

        if (commandInput.IsOverride && duplicateHistoryEntity is not null)
        {
            if (commandInput.Points < duplicateHistoryEntity.Points)
            {
                return new LoweringTheScoreException(duplicateHistoryEntity.Points);
            }

            return ValidationResult.Success;
        }

        if (duplicateHistoryEntity is not null)
        {
            return new StandardAlreadyExistsException(
                commandInput.StudentGuid,
                commandInput.StandardType
            );
        }

        return ValidationResult.Success;
    }
}

public sealed class AddStandardPointsCommand : ICommand<AddStandardPointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddStandardPointsCommandValidator _validator;

    public AddStandardPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddStandardPointsCommandValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddStandardPointsCommandPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ToResult<Unit>();
        }

        var student = await _applicationContext.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.StudentGuid == commandPayload.StudentGuid);

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
        }

        var standardsStudentHistoryEntity = new StandardsStudentHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            TeacherGuid = commandPayload.TeacherGuid,
            SemesterName = student.CurrentSemesterName,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            StandardType = commandPayload.StandardType,
            Comment = commandPayload.Comment,
        };

        if (commandPayload.IsOverride)
        {
            var points = await _applicationContext.StandardsStudentsHistory
                .AsNoTracking()
                .Where(
                    s =>
                        s.StudentGuid == commandPayload.StudentGuid
                        && s.StandardType == commandPayload.StandardType
                )
                .OrderByDescending(s => s.Points)
                .Select(s => s.Points)
                .FirstAsync();

            student.PointsForStandards -= points;
        }

        var adjustedStudentPointsAmount = AdjustStudentPointsAmount(
            student.PointsForStandards,
            standardsStudentHistoryEntity.Points
        );

        student.PointsForStandards = adjustedStudentPointsAmount;

        _applicationContext.StandardsStudentsHistory.Add(standardsStudentHistoryEntity);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        await StudentArchiver.TryArchiveStudentIfHisDebtIsClosed(student, _applicationContext);

        return Unit.Default;
    }

    private int AdjustStudentPointsAmount(int studentTotalPointsForStandards, int pointsToImplement)
    {
        int adjustedStudentPointsAmount;
        if (studentTotalPointsForStandards + pointsToImplement > MAX_POINTS_FOR_STANDARDS)
        {
            adjustedStudentPointsAmount = MAX_POINTS_FOR_STANDARDS;
        }
        else
        {
            adjustedStudentPointsAmount = studentTotalPointsForStandards + pointsToImplement;
        }

        return adjustedStudentPointsAmount;
    }
}
