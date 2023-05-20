namespace PhysEdJournal.Infrastructure.Commands;

internal struct ValidationResult
{
    public required bool IsSuccess { get; init; }
    public bool IsFailed => !IsSuccess;
    public Exception? ValidationException { get; init; }

    public static implicit operator ValidationResult(Exception exc)
    {
        return new ValidationResult
        {
            IsSuccess = false,
            ValidationException = new AggregateException(exc)
        };
    }

    public static ValidationResult Success { get; } = new ValidationResult { IsSuccess = true };
}