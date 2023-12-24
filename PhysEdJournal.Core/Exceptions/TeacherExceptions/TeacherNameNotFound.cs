namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public sealed class TeacherNameNotFoundException : Exception
{
    public TeacherNameNotFoundException(string name)
        : base($"No teacher with name: {name}") { }
}