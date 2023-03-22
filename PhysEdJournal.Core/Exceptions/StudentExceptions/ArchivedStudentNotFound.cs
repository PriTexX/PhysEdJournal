namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class ArchivedStudentNotFound : Exception
{
    public ArchivedStudentNotFound(string guid, string semesterName): base($"No archived student with guid: {guid} and semester name: {semesterName}"){}
}