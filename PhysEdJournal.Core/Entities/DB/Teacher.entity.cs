using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class TeacherEntity
{
    [StringLength(36)]
    [Key]
    public string TeacherGuid { get; set; }

    [StringLength(120)]
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }

    [DefaultValue(TeacherPermissions.DefaultAccess)]
    public TeacherPermissions Permissions { get; set; }

    public ICollection<GroupEntity>? Groups { get; set; }
}
