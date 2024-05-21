namespace PhysEdJournal.Core.Exceptions.SemesterExceptions;

public class SemesterAlreadyExistsException : Exception
{
    public SemesterAlreadyExistsException(string semesterName)
        : base($"Semester with name {semesterName} already exists") { }
}
