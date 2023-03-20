namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class NonRegularPointsValueException : Exception
{
    public NonRegularPointsValueException(int pointsValue) : base($"Value: {pointsValue} doesn't match value standards for this operation"){}
}