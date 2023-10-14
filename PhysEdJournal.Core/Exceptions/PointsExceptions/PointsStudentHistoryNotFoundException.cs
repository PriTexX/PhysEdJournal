namespace PhysEdJournal.Core.Exceptions.PointsExceptions;

public sealed class PointsStudentHistoryNotFoundException : Exception
{
    public PointsStudentHistoryNotFoundException(int historyId, string studentGuid)
        : base($"No history with id = {historyId} in student with guid {studentGuid}") { }
}
