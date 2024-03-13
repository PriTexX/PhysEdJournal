using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class SemesterEntity
{
    [StringLength(32)]
    [Required(AllowEmptyStrings = false)]
    [Key]
    public required string Name { get; set; }

    public bool IsCurrent { get; set; }

    public ICollection<ArchivedStudentEntity>? ArchivedStudents { get; set; }
}
