namespace PhysEdJournal.Core.Exceptions.GroupExceptions;

public class NullVisitValueException : Exception
{
    public NullVisitValueException(double visitValue)
        : base($"Visit value cannot be negative. Visit value: {visitValue}") { }
}
