namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public sealed class VisitExpiredException : Exception
{
    public VisitExpiredException(DateOnly date)
        : base($"The ability to set a visit for {date} has expired") { }
}
