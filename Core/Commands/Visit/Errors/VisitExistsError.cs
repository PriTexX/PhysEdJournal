namespace Core.Commands;

public sealed class VisitExistsError() : Exception("Visit for provided date already exists") { }
