namespace PhysEdJournal.Core.Exceptions.DateExceptions;

public class DateExpiredException : Exception
{
    public DateExpiredException(DateOnly date): base($"The ability to set points for {date} has expired") {}
}