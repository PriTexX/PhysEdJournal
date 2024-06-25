namespace Core.Commands;

public sealed class SameSemesterError()
    : Exception("Cannot migrate to new semester student as it already there") { }
