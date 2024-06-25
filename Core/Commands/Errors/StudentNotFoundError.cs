namespace Core.Commands;

public sealed class StudentNotFoundError() : Exception("Student with such guid not found") { }
