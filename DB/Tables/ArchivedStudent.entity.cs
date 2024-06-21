using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Tables;

public class ArchivedHistory
{
    public required DateOnly Date { get; set; }
    public required double Points { get; set; }
    public required string TeacherGuid { get; set; }
    public required string StudentGuid { get; set; }
}

public sealed class ArchivedPointsHistory : ArchivedHistory
{
    public required WorkType WorkType { get; set; }
    public string? Comment { get; set; }
}

public sealed class ArchivedStandardsHistory : ArchivedHistory
{
    public required StandardType StandardType { get; set; }
    public string? Comment { get; set; }
}

public sealed class ArchivedStudentEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string StudentGuid { get; set; }

    [StringLength(32)]
    [Required(AllowEmptyStrings = false)]
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

    [DefaultValue(0)]
    public required int Visits { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<ArchivedHistory> VisitsHistory { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<ArchivedPointsHistory> PointsHistory { get; set; }

    [Column(TypeName = "jsonb")]
    public required List<ArchivedStandardsHistory> StandardsHistory { get; set; }
}
