namespace PhysEdJournal.Core.Exceptions.VisitsExceptions;

public class VisitsStudentHistoryNotFoundException : Exception
{
    public VisitsStudentHistoryNotFoundException(int historyId)
        : base($"No visits history record with id = {historyId}") { }
}
