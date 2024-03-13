using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PhysEdJournal.Core.Entities.Types;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class TeacherEntity
{
    [StringLength(GuidLength)]
    [Key]
    public required string TeacherGuid { get; set; }

    [StringLength(120)]
    [Required(AllowEmptyStrings = false)]
    public required string FullName { get; set; }

    [DefaultValue(TeacherPermissions.DefaultAccess)]
    public TeacherPermissions Permissions { get; set; }

    public ICollection<GroupEntity>? Groups { get; set; }
}
