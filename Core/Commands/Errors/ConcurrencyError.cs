namespace Core.Commands;

public sealed class ConcurrencyError()
    : Exception("Concurrency error happened due to parallel update of one student") { }
