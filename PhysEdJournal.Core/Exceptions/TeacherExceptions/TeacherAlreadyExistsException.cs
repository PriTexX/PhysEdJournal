namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public class TeacherAlreadyExistsException : Exception
{
    public TeacherAlreadyExistsException(string guid): base($"Teacher with guid: {guid} already exists"){}
}