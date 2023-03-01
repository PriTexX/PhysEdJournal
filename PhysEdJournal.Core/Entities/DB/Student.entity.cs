using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class StudentEntity
{
    [Key]
    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string GroupNumber { get; set; }
    
    [DefaultValue(false)]
    public bool HasDebtFromPreviousSemester { get; set; } // Если студент не получил зачет из-за нехватки баллов
     
    [DefaultValue(0)]
    public double ArchivedVisitValue { get; set; }  // Сохраняем стоимость посещения в прошлом семестре здесь, чтобы считать по ней баллы, пока не набертся 50
                                                    // После набора 50 баллов и получения зачета вернуть в Null, а HasDebt в false
    [DefaultValue(0)]
    public int AdditionalPoints { get; set; }
    
    [ForeignKey("GroupNumber")]
    public GroupEntity? Group { get; set; }

    [DefaultValue(0)]
    public int Visits { get; set; }
    
    [Required]
    [Range(1, 6)]
    public int Course { get; set; }
    
    public int? HealthGroup { get; set; }

    public string? Department { get; set; }
    
    public ICollection<PointsStudentHistoryEntity>? PointsStudentHistory { get; set; }
    
    public ICollection<VisitStudentHistoryEntity>? VisitsStudentHistory { get; set; }
}