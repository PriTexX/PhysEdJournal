namespace Core.Commands;

public sealed class OutOfStandardsPointsLimitError()
    : Exception("Student already has maximum points for standards") { }
