using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Tables;

[Table("Semesters")]
public sealed class SemesterEntity
{
    [Key]
    [StringLength(32)]
    public required string Name { get; set; }

    public bool IsCurrent { get; set; }

    public ICollection<ArchivedStudentEntity>? ArchivedStudents { get; set; }
}
