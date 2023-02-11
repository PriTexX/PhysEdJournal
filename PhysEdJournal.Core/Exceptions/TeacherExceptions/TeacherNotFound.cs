namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public sealed class TeacherNotFound : Exception
{
    public TeacherNotFound(string guid): base($"No teacher with guid: {guid}"){}
}