namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class FitnessAlreadyExistsException : Exception
{
    public FitnessAlreadyExistsException()
        : base("Student already has points for external fitness") { }
}
