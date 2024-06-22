namespace Core.Commands;

public sealed class HistoryDeleteExpiredError()
    : Exception("Time to delete history has expired") { }
