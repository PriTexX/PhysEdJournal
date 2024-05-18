namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class NotCuratorError : Exception
{
    public NotCuratorError()
        : base("Teacher has to be a curator of the group to perform this action") { }
}
