using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class PointsStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "date")]
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public int Points { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]
    public string SemesterName { get; set; }
    
    [ForeignKey("SemesterName")]
    public SemesterEntity Semester { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string TeacherGuid { get; set; }
    
    [ForeignKey("TeacherGuid")]
    
    public TeacherEntity Teacher { get; set; }

    [Required]
    public WorkType WorkType { get; set; }
    
    [DefaultValue(false)]
    public bool IsArchived { get; set; }
    
    public string? Comment { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [ForeignKey("StudentGuid")]
    public StudentEntity Student { get; set; }
}