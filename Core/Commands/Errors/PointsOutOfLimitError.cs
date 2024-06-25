namespace Core.Commands;

public sealed class PointsOutOfLimitError() : Exception("Provided points are out of limit") { }
