namespace Core.Commands;

public sealed class NotEnoughPointsForStandardsError()
    : Exception("Student does not have enough points to pass standards") { }
