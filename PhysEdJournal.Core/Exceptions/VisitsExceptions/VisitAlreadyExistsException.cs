namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public class VisitAlreadyExistsException : Exception
{
    public VisitAlreadyExistsException(DateOnly date)
        : base($"Visit for {date} already exists") { }
}
