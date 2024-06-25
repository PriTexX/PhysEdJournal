namespace Core.Commands;

public class CuratorMismatchError() : Exception("Teacher must be curator of a student") { }
