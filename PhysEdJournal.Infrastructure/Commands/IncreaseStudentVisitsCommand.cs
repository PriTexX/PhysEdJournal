using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Constants;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class IncreaseStudentVisitsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required bool IsAdmin { get; init; }
    public required DateOnly Date { get; init; }
}

internal sealed class IncreaseStudentVisitsCommandValidator
    : ICommandValidator<IncreaseStudentVisitsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public IncreaseStudentVisitsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        IncreaseStudentVisitsCommandPayload commandInput
    )
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureException(commandInput.Date);
        }

        if (commandInput.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayException(commandInput.Date.DayOfWeek);
        }

        if (
            DateOnly.FromDateTime(DateTime.Now).DayNumber - commandInput.Date.DayNumber
                > VisitConstants.VISIT_LIFE_DAYS
            && !commandInput.IsAdmin
        )
        {
            return new VisitExpiredException(commandInput.Date);
        }

        var recordCopy = await _applicationContext.VisitsStudentsHistory
            .Where(v => v.StudentGuid == commandInput.StudentGuid && v.Date == commandInput.Date)
            .FirstOrDefaultAsync();

        if (recordCopy is not null)
        {
            return new VisitAlreadyExistsException(commandInput.Date);
        }

        return ValidationResult.Success;
    }
}

public sealed class IncreaseStudentVisitsCommand
    : ICommand<IncreaseStudentVisitsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly IncreaseStudentVisitsCommandValidator _validator;

    public IncreaseStudentVisitsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new IncreaseStudentVisitsCommandValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(IncreaseStudentVisitsCommandPayload commandPayload)
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

        student.Visits++;

        var record = new VisitStudentHistoryEntity
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
            return new Result<Unit>(new ConcurrencyError());
        }

        await StudentArchiver.TryArchiveStudentIfHisDebtIsClosed(student, _applicationContext);

        return Unit.Default;
    }
}
