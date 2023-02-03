namespace PhysEdJournal.Core.Entities.Types;

[Flags]
public enum TeacherPermissions
{
    DefaultAccess = 0,
    AdminAccess = 1,
    SecretaryAccess = 2,
    OnlineCourseAccess = 4
}