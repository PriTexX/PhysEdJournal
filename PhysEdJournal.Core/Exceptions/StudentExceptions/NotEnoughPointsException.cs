namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class NotEnoughPointsException : Exception
{
    public NotEnoughPointsException(string guid) : base($"Student with guid: {guid} does not have enough points"){}
}