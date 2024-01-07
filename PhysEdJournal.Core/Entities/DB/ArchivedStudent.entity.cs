using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class ArchivedStudentEntity
{
    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string StudentGuid { get; set; }

    [StringLength(32)]
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]
    public required string SemesterName { get; set; }

    [ForeignKey("SemesterName")]
    public SemesterEntity? Semester { get; set; }

    [StringLength(120)]
    [Required(AllowEmptyStrings = false)]
    public required string FullName { get; set; }

    [StringLength(20)]
    [Required(AllowEmptyStrings = false)]
    public required string GroupNumber { get; set; }

    [ForeignKey("GroupNumber")]
    public GroupEntity? Group { get; set; }

    [Required]
    public required double TotalPoints { get; set; }

    [DefaultValue(0)]
    public required int Visits { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<VisitStudentHistoryEntity> VisitsHistory { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<PointsStudentHistoryEntity> PointsHistory { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<StandardsStudentHistoryEntity> StandardsHistory { get; set; }
}
