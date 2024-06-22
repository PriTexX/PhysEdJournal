namespace Core.Commands;

public sealed class TeacherNotFoundError() : Exception("Teacher with such guid not found") { }
