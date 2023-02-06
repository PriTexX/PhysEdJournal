using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public class StudentEntity
{
    [Key]
    [Required]
    public string StudentGuid { get; set; }
    
    [Required] 
    public string FullName { get; set; }
    
    [Required]
    public string GroupNumber { get; set; }
    
    [DefaultValue(0)]
    [NotMapped]
    public double TotalPoints => (Visits * Group.VisitValue) + AdditionalPoints;

    [DefaultValue(0)]
    public int AdditionalPoints { get; set; }
    
    [ForeignKey("GroupNumber")]
    public GroupEntity Group { get; set; }

    [DefaultValue(0)]
    public int Visits { get; set; }
    
    [Required]
    [Range(1, 6)]
    public int Course { get; set; }
    
    public int? HealthGroup { get; set; }

    public string? Department { get; set; }
    
    public ICollection<StudentPointsHistoryEntity> StudentPointsHistory { get; set; }
    public ICollection<StudentVisitsHistoryEntity> StudentVisitsHistory { get; set; }
}