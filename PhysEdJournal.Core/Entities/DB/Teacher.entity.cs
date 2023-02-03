using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public class TeacherEntity
{
    [Key]
    public string TeacherGuid { get; set; }
    
    [Required]
    public string FullName { get; set; }
    
    public ICollection<GroupEntity> Groups { get; set; }
}