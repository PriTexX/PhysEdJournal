namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class NegativePointAmount : Exception
{
    public NegativePointAmount()
        : base("Cannot grant negative or 0 amount of points") { }
}
