namespace Core.Commands;

public sealed class GTOExistsError() : Exception("GTO already exists for student") { }
