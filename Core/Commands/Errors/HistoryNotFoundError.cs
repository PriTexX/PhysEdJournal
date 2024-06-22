namespace Core.Commands;

public sealed class HistoryNotFoundError() : Exception("History with such id not found") { }
