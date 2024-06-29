using Core.Config;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class AddPointsPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required WorkType WorkType { get; init; }
    public required bool IsAdminOrSecretary { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddPointsValidator : ICommandValidator<AddPointsPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddPointsValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddPointsPayload input)
    {
        if (input.Points > Cfg.MaxPointsAmount)
        {
            return new PointsOutOfLimitError();
        }

        if (input.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ActionFromFutureError();
        }

        var student = await _applicationContext
            .Students.Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.StudentGuid == input.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        if (input.WorkType == WorkType.ExternalFitness)
        {
            var anotherFitness = await _applicationContext
                .PointsStudentsHistory.AsNoTracking()
                .Where(p =>
                    p.StudentGuid == input.StudentGuid && p.WorkType == WorkType.ExternalFitness
                )
                .FirstOrDefaultAsync();

            if (anotherFitness is not null)
            {
                return new FitnessExistsError();
            }

            if (input.Points > Cfg.MaxPointsForExternalFitness)
            {
                return new PointsOutOfLimitError();
            }
        }

        if (input.WorkType == WorkType.GTO)
        {
            var anotherGTO = await _applicationContext
                .PointsStudentsHistory.AsNoTracking()
                .Where(p => p.StudentGuid == input.StudentGuid && p.WorkType == WorkType.GTO)
                .FirstOrDefaultAsync();

            if (anotherGTO is not null)
            {
                return new GTOExistsError();
            }
        }

        if (input.WorkType == WorkType.Science && input.Points > Cfg.MaxPointsForScience)
        {
            return new PointsOutOfLimitError();
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - input.Date.DayNumber
                > Cfg.PointsLifeDays
            && !input.IsAdminOrSecretary
        )
        {
            return new DateExpiredError();
        }

        if (input.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayError();
        }

        return ValidationResult.Success;
    }
}

public sealed class AddPointsCommand : ICommand<AddPointsPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddPointsValidator _validator;

    public AddPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddPointsValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddPointsPayload payload)
    {
        var validation = await _validator.ValidateCommandInputAsync(payload);

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await _applicationContext
            .Students.Include(s => s.Group)
            .FirstAsync(s => s.StudentGuid == payload.StudentGuid);

        var pointsStudentHistoryEntity = new PointsHistoryEntity
        {
            StudentGuid = payload.StudentGuid,
            Comment = payload.Comment,
            Date = payload.Date,
            Points = payload.Points,
            WorkType = payload.WorkType,
            TeacherGuid = payload.TeacherGuid,
        };

        student.AdditionalPoints += pointsStudentHistoryEntity.Points;

        _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
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
}
