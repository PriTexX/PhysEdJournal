namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class NotEnoughPoints : Exception
{
    public NotEnoughPoints(string guid) : base($"Student with guid: {guid} does not have enough points"){}
}