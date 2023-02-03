namespace PhysEdJournal.Core.Entities.DB;

[Flags]
public enum TeacherPermissions
{
    DefaultAccess = 0,
    AdminAccess = 1,
    SecretaryAccess = 2,
    OnlineCourseAccess = 4
}