namespace PhysEdJournal.Core.Exceptions.GroupExceptions;

public class WrongVisitValueException : Exception
{
    public WrongVisitValueException(double visitValue)
        : base($"Visit value cannot be negative. Visit value: {visitValue}") { }
}
