namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public class PointsExpiredException : Exception
{
    public PointsExpiredException(DateOnly date): base($"The ability to set points for {date} has expired") {}
}