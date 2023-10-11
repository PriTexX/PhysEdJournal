namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public class GTOAlreadyExistsException : Exception
{
    public GTOAlreadyExistsException()
        : base("Student already has points for GTO") { }
}
