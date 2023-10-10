namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class NotEnoughPointsException : Exception
{
    public NotEnoughPointsException(string guid, double points)
        : base($"Student with guid: {guid} does not have enough points. Current points: {points}")
    { }
}
