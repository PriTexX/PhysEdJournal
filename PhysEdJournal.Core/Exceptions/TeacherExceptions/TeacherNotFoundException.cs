namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public sealed class TeacherNotFoundException : Exception
{
    public TeacherNotFoundException(string guid): base($"No teacher with guid: {guid}"){}
}