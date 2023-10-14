namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public sealed class ArchivedPointsDeletionException : Exception
{
    public ArchivedPointsDeletionException()
        : base("You can't delete archived points") { }
}
