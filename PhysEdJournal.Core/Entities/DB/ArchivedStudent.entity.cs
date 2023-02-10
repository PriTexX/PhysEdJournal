using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public class ArchivedStudentEntity
{
    [Key]
    [Required]
    public string StudentGuid { get; set; }
    
    [Required] 
    public string FullName { get; set; }
    
    [Required]
    public string GroupNumber { get; set; }
    
    [ForeignKey("GroupNumber")]
    public GroupEntity Group { get; set; }
    
    [Required]
    public double TotalPoints { get; set; }

    [DefaultValue(0)]
    public int Visits { get; set; }

    public ICollection<StudentPointsHistoryEntity> StudentPointsHistory { get; set; }
    public ICollection<StudentVisitsHistoryEntity> StudentVisitsHistory { get; set; }
}