namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public class CannotMigrateToNewSemesterException : Exception
{
    public CannotMigrateToNewSemesterException(string semester)
        : base(
            $"Cannot archive student with semester: {semester} and migrate to an active semester as he already has an active semester"
        ) { }
}
