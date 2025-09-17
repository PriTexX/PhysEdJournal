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

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddPointsPayload payload)
    {
        if (payload.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ActionFromFutureError();
        }

        var student = await _applicationContext
            .Students.Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.StudentGuid == payload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        if (
            payload.WorkType is WorkType.Science or WorkType.GTO
            && payload.Date != DateOnly.FromDateTime(DateTime.UtcNow)
        )
        {
            return new DateExpiredError();
        }

        if (payload.WorkType == WorkType.ExternalFitness)
        {
            var anotherFitness = await _applicationContext
                .PointsStudentsHistory.AsNoTracking()
                .Where(p =>
                    p.StudentGuid == payload.StudentGuid && p.WorkType == WorkType.ExternalFitness
                )
                .FirstOrDefaultAsync();

            if (anotherFitness is not null)
            {
                return new FitnessExistsError();
            }

            if (payload.Points > Cfg.MaxPointsForExternalFitness)
            {
                return new PointsOutOfLimitError();
            }
        }

        if (payload.WorkType == WorkType.GTO)
        {
            var anotherGTO = await _applicationContext
                .PointsStudentsHistory.AsNoTracking()
                .Where(p => p.StudentGuid == payload.StudentGuid && p.WorkType == WorkType.GTO)
                .FirstOrDefaultAsync();

            if (anotherGTO is not null)
            {
                return new GTOExistsError();
            }
        }

        if (payload.WorkType == WorkType.Science && payload.Points > Cfg.MaxPointsForScience)
        {
            return new PointsOutOfLimitError();
        }

        var daysPastFromNow =
            DateOnly.FromDateTime(DateTime.Now).DayNumber - payload.Date.DayNumber;

        if (
            payload.WorkType == WorkType.OnlineWork
            && daysPastFromNow > Cfg.OnlineWorkPointsLifeDays
            && !payload.IsAdminOrSecretary
        )
        {
            return new DateExpiredError();
        }

        if (daysPastFromNow > Cfg.PointsLifeDays && !payload.IsAdminOrSecretary)
        {
            return new DateExpiredError();
        }

        if (payload.Date.DayOfWeek is DayOfWeek.Sunday)
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

        var student = await _applicationContext.Students.FirstAsync(s =>
            s.StudentGuid == payload.StudentGuid
        );

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
