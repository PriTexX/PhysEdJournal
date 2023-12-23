using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class VisitStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "date")]
    [Required(AllowEmptyStrings = false)]
    public DateOnly Date { get; set; }

    [StringLength(GuidLength)]
    [Required(AllowEmptyStrings = false)]
    public required string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity? Teacher { get; set; }

    [StringLength(GuidLength)]
    [Required(AllowEmptyStrings = false)]
    public required string StudentGuid { get; set; }

    [ForeignKey("StudentGuid")]
    public StudentEntity? Student { get; set; }
}
