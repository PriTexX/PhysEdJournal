using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class VisitsStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "date")]
    [Required(AllowEmptyStrings = false)]
    public DateOnly Date { get; set; }
    
    [Required]
    public SportType Sport { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string TeacherGuid { get; set; }
    
    [DefaultValue(false)]
    public bool IsArchived { get; set; }
    
    [ForeignKey("TeacherGuid")]
    public TeacherEntity Teacher { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [ForeignKey("StudentGuid")]
    public StudentEntity Student { get; set; }
}