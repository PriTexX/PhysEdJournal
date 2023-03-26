namespace PhysEdJournal.Core.Exceptions.DateExceptions;

public class ActionFromFutureException : Exception
{
    public ActionFromFutureException(DateOnly date): base($"{date} is later than current date"){}
}