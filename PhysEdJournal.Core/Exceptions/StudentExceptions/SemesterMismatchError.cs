namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class SemesterMismatchError : Exception
{
    public SemesterMismatchError()
        : base("Cannot migrate student to active semester as it already there") { }
}
