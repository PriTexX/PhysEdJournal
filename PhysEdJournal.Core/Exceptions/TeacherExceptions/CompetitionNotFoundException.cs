namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public class CompetitionNotFoundException : Exception
{
    public CompetitionNotFoundException(string compName): base($"Competition: {compName} was not found")
    {}
}