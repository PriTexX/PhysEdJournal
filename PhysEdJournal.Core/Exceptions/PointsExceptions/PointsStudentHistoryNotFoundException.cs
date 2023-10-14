namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public sealed class PointsStudentHistoryNotFoundException : Exception
{
    public PointsStudentHistoryNotFoundException(int historyId)
        : base($"No history with id = {historyId}") { }
}
