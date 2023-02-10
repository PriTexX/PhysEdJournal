using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public class StudentVisitsHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "date")]
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public SportType Sport { get; set; }
    
    [Required]
    public GymType Gym { get; set; }

    [Required]
    public string TeacherGuid { get; set; }
    
    [DefaultValue(false)]
    public bool IsArchived { get; set; }
    
    [ForeignKey("TeacherGuid")]
    public TeacherEntity Teacher { get; set; }

    [Required]
    public string StudentGuid { get; set; }
    
    [ForeignKey("StudentGuid")]
    public StudentEntity Student { get; set; }
}