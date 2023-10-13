namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public sealed class PointsOutdatedException: Exception
{
    public PointsOutdatedException(int amountOfDays) 
        : base($"You cannot delete points if they existed more than {amountOfDays} days ago") {}
}