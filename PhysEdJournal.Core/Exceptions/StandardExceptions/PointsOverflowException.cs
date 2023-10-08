namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public sealed class PointsOverflowException : Exception
{
    public PointsOverflowException(int pointsToAdd, int maxPoints)
        : base($"You can't add {pointsToAdd} points. Max possible value is {maxPoints}") { }
}
