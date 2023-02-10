using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public class TeacherEntity
{
    [Key]
    public string TeacherGuid { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }
    
    [DefaultValue(TeacherPermissions.DefaultAccess)]
    public TeacherPermissions Permissions { get; set; }
    
    public ICollection<GroupEntity>? Groups { get; set; }
}