namespace PhysEdJournal.Core.Exceptions.StudentExceptions;

public sealed class CuratorGuidMismatch : Exception
{
    public CuratorGuidMismatch()
        : base($"Teacher must be curator of a student") { }
}
