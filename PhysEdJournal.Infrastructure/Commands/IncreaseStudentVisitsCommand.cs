using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class IncreaseStudentVisitsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required DateOnly Date { get; init; }
}


internal sealed class IncreaseStudentVisitsCommandValidator
{
    public ValidationResult ValidateCommandInput(IncreaseStudentVisitsCommandPayload input)
    {
        if (input.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureException(input.Date);
        }

        if (DateOnly.FromDateTime(DateTime.Now).DayNumber - input.Date.DayNumber > VISIT_LIFE_DAYS)
        {
            return new VisitExpiredException(input.Date);
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
        _validator = new IncreaseStudentVisitsCommandValidator();
    }

    public async Task<Result<Unit>> ExecuteAsync(IncreaseStudentVisitsCommandPayload commandPayload)
    {
        var validationResult = _validator.ValidateCommandInput(commandPayload);

        if (validationResult.IsFailed)
        {
            return new Result<Unit>(validationResult.ValidationException);
        }

        return new Result<Unit>(Unit.Default);
    }
}

