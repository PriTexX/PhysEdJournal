namespace PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;

internal interface ICommandValidator<TPayload> where TPayload : class
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(TPayload commandInput);
}