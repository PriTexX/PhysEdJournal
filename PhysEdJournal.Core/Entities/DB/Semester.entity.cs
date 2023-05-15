using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class SemesterEntity
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]  // 2022-2023/весна
    [Key]
    public string Name { get; set; }
    
    public bool IsCurrent { get; set; }

    public ICollection<ArchivedStudentEntity> ArchivedStudents { get; set; }
    public ICollection<PointsStudentHistoryEntity> StudentPointsHistory { get; set; }
}