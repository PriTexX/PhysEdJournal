namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public class ArchivedVisitDeletionException : Exception
{
    public ArchivedVisitDeletionException()
        : base("You can't delete archived visits") { }
}
