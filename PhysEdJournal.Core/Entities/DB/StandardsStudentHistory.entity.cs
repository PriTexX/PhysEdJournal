using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public class StandardsStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Range(0,10)]
    public int Points { get; set; }
    
    [Column(TypeName = "date")]
    [Required(AllowEmptyStrings = false)]
    public DateOnly Date { get; set; }
    
    [Required]
    public StandardType StandardType { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity Teacher { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [ForeignKey("StudentGuid")]
    public StudentEntity Student { get; set; }
}