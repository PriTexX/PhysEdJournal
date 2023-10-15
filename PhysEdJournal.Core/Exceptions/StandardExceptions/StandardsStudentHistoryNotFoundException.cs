namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public class StandardsStudentHistoryNotFoundException : Exception
{
    public StandardsStudentHistoryNotFoundException(int historyId)
        : base($"No standards history record with id = {historyId}") { }
}
