namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class StudentNotFound : Exception
{
    public StudentNotFound(string guid): base($"No student with guid: {guid}"){}
}