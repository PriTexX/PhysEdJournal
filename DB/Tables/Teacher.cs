using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Tables;

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

[Table("Teachers")]
public sealed class TeacherEntity
{
    [Key]
    [StringLength(36)]
    public required string TeacherGuid { get; set; }

    [StringLength(120)]
    public required string FullName { get; set; }

    public required TeacherPermissions Permissions { get; set; }

    public ICollection<GroupEntity>? Groups { get; set; }
}
