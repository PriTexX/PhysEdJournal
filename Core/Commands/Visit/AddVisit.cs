using Core.Cfg;
using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class AddVisitPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required bool IsAdminOrSecretary { get; init; }
    public required DateOnly Date { get; init; }
}

internal sealed class AddVisitValidator : ICommandValidator<AddVisitPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddVisitValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddVisitPayload commandInput)
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureError();
        }

        if (commandInput.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayError();
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - commandInput.Date.DayNumber
                > Config.VisitLifeDays
            && !commandInput.IsAdminOrSecretary
        )
        {
            return new DateExpiredError();
        }

        var recordCopy = await _applicationContext
            .VisitsStudentsHistory.Where(v =>
                v.StudentGuid == commandInput.StudentGuid && v.Date == commandInput.Date
            )
            .FirstOrDefaultAsync();

        if (recordCopy is not null)
        {
            return new VisitExistsError();
        }

        return ValidationResult.Success;
    }
}

public sealed class AddVisitCommand : ICommand<AddVisitPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddVisitValidator _validator;

    public AddVisitCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddVisitValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddVisitPayload commandPayload)
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

        student.Visits++;

        var record = new VisitsHistoryEntity
        {
            Date = commandPayload.Date,
            StudentGuid = commandPayload.StudentGuid,
            TeacherGuid = commandPayload.TeacherGuid,
        };

        _applicationContext.VisitsStudentsHistory.Add(record);
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
