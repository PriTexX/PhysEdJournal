namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public class VisitOutdatedException : Exception
{
    public VisitOutdatedException(int amountOfDays)
        : base($"You cannot delete a visit if it existed more than {amountOfDays} days ago") { }
}
