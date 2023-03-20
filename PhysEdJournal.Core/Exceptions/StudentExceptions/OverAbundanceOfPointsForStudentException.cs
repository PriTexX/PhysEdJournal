namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class OverAbundanceOfPointsForStudentException : Exception
{
    public OverAbundanceOfPointsForStudentException(string guid) : base($"You can't add more points for student with guid: {guid}"){}
}