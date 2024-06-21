using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

public sealed class TeacherEntity
{
    [StringLength(36)]
    [Key]
    public required string TeacherGuid { get; set; }

    [StringLength(120)]
    [Required(AllowEmptyStrings = false)]
    public required string FullName { get; set; }

    [DefaultValue(TeacherPermissions.DefaultAccess)]
    public TeacherPermissions Permissions { get; set; }

    public ICollection<GroupEntity>? Groups { get; set; }
}
