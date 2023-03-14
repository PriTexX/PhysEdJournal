using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Exceptions.TeacherExceptions;

public class NotEnoughPermissionsException : Exception
{
    public NotEnoughPermissionsException(string teacherGuid, TeacherPermissions permissions, TeacherPermissions requiredPermissions) :
        base($"Teacher with guid: {teacherGuid} does not have enough permissions. Required permissions: {requiredPermissions} - teacher permissions: {permissions}"){}
}