using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class GroupEntity
{
    [StringLength(30)]
    [Key]
    public required string GroupName { get; set; }

    [DefaultValue(2.0)]
    public double VisitValue { get; set; } = 2.0;

    [StringLength(GuidLength)]
    public string? CuratorGuid { get; set; }

    [ForeignKey("CuratorGuid")]
    public TeacherEntity? Curator { get; set; }

    public ICollection<StudentEntity>? Students { get; set; }
}
