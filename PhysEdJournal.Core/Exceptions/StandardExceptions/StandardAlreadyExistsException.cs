using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Exceptions.StandardExceptions;

public class StandardAlreadyExistsException : Exception
{
    public StandardAlreadyExistsException(string studentGuid, StandardType type)
        : base($"Student with guid {studentGuid} has already passed the standard of type {type}")
    { }
}
