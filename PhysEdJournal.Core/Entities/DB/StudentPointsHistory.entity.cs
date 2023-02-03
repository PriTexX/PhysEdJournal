using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public class StudentPointsHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "date")]
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public WorkType WorkType { get; set; }
    
    [Required]
    public int Points { get; set; }
    
    [Required]
    public string StudentGuid { get; set; }
    
    [ForeignKey("StudentGuid")]
    public StudentEntity Student { get; set; }
}