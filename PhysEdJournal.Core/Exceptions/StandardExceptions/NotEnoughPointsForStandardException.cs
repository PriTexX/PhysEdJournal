namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public class NotEnoughPointsForStandardException : Exception
{
    public NotEnoughPointsForStandardException()
        : base("Student must have more than 20 points to pass a standard") { }
}
