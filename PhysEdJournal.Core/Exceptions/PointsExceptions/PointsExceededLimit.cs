namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public class PointsExceededLimit : Exception
{
    public PointsExceededLimit(int maxValue) : base($"The maximum points value is {maxValue}")
    {}
}