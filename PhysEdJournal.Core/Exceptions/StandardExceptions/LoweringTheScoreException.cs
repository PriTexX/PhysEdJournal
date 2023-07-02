namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public sealed class LoweringTheScoreException : Exception
{
    public LoweringTheScoreException(int pointsForPreviousTry) : base($"You can't add less points than a student already has. The student received {pointsForPreviousTry} for his last try."){}
}