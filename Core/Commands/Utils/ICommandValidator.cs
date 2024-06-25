namespace Core.Commands;

internal interface ICommandValidator<in TPayload>
    where TPayload : class
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(TPayload commandInput);
}
