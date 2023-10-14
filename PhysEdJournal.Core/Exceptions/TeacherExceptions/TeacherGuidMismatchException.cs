namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public sealed class TeacherGuidMismatchException : Exception
{
    public TeacherGuidMismatchException(string teacherGuid)
        : base($"This action was made by other teacher with guid {teacherGuid}") { }
}
