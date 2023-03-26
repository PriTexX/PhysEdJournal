namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public class NonRegularPointsValueException : Exception
{
    public NonRegularPointsValueException(int pointsValue) : base($"Value: {pointsValue} doesn't match value standards for this operation"){}
}