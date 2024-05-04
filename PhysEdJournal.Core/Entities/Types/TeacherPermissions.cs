using System.Text.Json.Serialization;

namespace PhysEdJournal.Core.Entities.Types;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TeacherPermissions
{
    DefaultAccess = 0,
    SuperUser = 1,
    AdminAccess = 2,
    SecretaryAccess = 4,
    OnlineCourseAccess = 8,
}
