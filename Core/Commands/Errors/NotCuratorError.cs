namespace Core.Commands;

public sealed class NotCuratorError() : Exception("Teacher is not curator for this group") { }
