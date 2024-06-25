namespace Core.Commands;

public sealed class GroupNotFoundError() : Exception("Group with such number not found") { }
