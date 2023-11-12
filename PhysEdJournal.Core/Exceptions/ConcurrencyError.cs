namespace PhysEdJournal.Core.Exceptions;

public sealed class ConcurrencyError : Exception
{
    public ConcurrencyError()
        : base("Concurrency error happened due to parallel update of one student") { }
}
