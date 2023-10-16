namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public sealed class PointsStudentHistoryNotFoundException : Exception
{
    public PointsStudentHistoryNotFoundException(int historyId)
        : base($"No points history record with id = {historyId}") { }
}
