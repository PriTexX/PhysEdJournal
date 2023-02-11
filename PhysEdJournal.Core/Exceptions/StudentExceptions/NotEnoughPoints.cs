namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class NotEnoughPoints : Exception
{
    public NotEnoughPoints(string guid) : base($"Student with guid: {guid} does not have enough points"){}
}