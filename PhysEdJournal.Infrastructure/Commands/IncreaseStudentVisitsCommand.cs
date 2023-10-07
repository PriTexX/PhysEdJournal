using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class IncreaseStudentVisitsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required DateOnly Date { get; init; }
}


internal sealed class IncreaseStudentVisitsCommandValidator : ICommandValidator<IncreaseStudentVisitsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public IncreaseStudentVisitsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(IncreaseStudentVisitsCommandPayload input)
    {
        if (input.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureException(input.Date);
        }
        
        if (input.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
        {
            return new NonWorkingDayException(input.Date.DayOfWeek);
        }

        // if (DateOnly.FromDateTime(DateTime.Now).DayNumber - input.Date.DayNumber > VISIT_LIFE_DAYS)
        // {
        //     return new VisitExpiredException(input.Date);
        // }
        
        var recordCopy = await _applicationContext.VisitsStudentsHistory
            .Where(v => v.StudentGuid == input.StudentGuid && v.Date == input.Date)
            .FirstOrDefaultAsync();

        if (recordCopy is not null)
        {
            return new VisitAlreadyExistsException(input.Date);
        }

        return ValidationResult.Success;
    }
} 


public sealed class IncreaseStudentVisitsCommand : ICommand<IncreaseStudentVisitsCommandPayload, Unit>
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
        
        var student = await _applicationContext.Students.FindAsync(commandPayload.StudentGuid);

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
        }
        
        student.Visits++;

        var record = new VisitStudentHistoryEntity
        {
            Date = commandPayload.Date,
            StudentGuid = commandPayload.StudentGuid,
            TeacherGuid = commandPayload.TeacherGuid
        };

        _applicationContext.VisitsStudentsHistory.Add(record);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        await StudentArchiver.TryArchiveStudentIfHisDebtIsClosed(student, _applicationContext);
        
        return Unit.Default;
    }
}

