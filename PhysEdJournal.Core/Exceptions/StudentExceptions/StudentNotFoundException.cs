namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class StudentNotFoundException : Exception
{
    public StudentNotFoundException(string guid)
        : base($"No student with guid: {guid}") { }
}
