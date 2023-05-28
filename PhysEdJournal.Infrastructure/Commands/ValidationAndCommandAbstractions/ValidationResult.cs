namespace PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;

internal struct ValidationResult
{
    public required bool IsSuccess { get; init; }
    public bool IsFailed => !IsSuccess;
    public Exception? ValidationException { get; init; }

    public static ValidationResult Create(Exception exc)
    {
        return new ValidationResult
        {
            IsSuccess = false,
            ValidationException = new AggregateException(exc)
        };
    }
    
    public static implicit operator ValidationResult(Exception exc)
    {
        return new ValidationResult
        {
            IsSuccess = false,
            ValidationException = new AggregateException(exc)
        };
    }

    public static implicit operator ValueTask<ValidationResult>(ValidationResult result)
    {
        return ValueTask.FromResult(result);
    }

    public static ValidationResult Success { get; } = new ValidationResult { IsSuccess = true };
}