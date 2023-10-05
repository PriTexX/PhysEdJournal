using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class AssignVisitValueCommandPayload
{
    public required string GroupName { get; init; }
    public required double NewVisitValue { get; init; }
}

internal sealed class AssignVisitValueCommandValidator : ICommandValidator<AssignVisitValueCommandPayload>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(AssignVisitValueCommandPayload commandInput)
    {
        if (commandInput.NewVisitValue <= 0)
        {
            return ValueTask.FromResult<ValidationResult>(new NullVisitValueException(commandInput.NewVisitValue));
        }

        return ValueTask.FromResult(ValidationResult.Success);
    }
} 

public sealed class AssignVisitValueCommand : ICommand<AssignVisitValueCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AssignVisitValueCommandValidator _validator;

    public AssignVisitValueCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AssignVisitValueCommandValidator();
    }
    
    public async Task<Result<Unit>> ExecuteAsync(AssignVisitValueCommandPayload commandPayload)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validationResult.IsFailed)
        {
            return validationResult.ToResult<Unit>();
        }
        
        var group = await _applicationContext.Groups.FindAsync(commandPayload.GroupName);

        if (group is null)
        {
            return new Result<Unit>(new GroupNotFoundException(commandPayload.GroupName));
        }

        group.VisitValue = commandPayload.NewVisitValue;

        _applicationContext.Groups.Update(group);
        await _applicationContext.SaveChangesAsync();

        return new Result<Unit>(Unit.Default);
    }
}