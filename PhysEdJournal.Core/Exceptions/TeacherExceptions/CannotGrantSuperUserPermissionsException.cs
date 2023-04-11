namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public class CannotGrantSuperUserPermissionsException : Exception
{
    public CannotGrantSuperUserPermissionsException(string guid) : base($"Cannot grant superuser permissions to teacher: {guid}") {}
}