using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public class GroupEntity
{
    [Key]
    public string GroupName { get; set; }
    
    [DefaultValue(2.0)]
    public double VisitValue { get; set; }
    
    public string? CuratorGuid { get; set; }
    
    [ForeignKey("CuratorGuid")]
    public TeacherEntity? Curator { get; set; }
    
    public ICollection<StudentEntity> Students { get; set; }
}