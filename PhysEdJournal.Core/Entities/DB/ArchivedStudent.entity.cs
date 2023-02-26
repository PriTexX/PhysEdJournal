using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class ArchivedStudentEntity
{
    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]
    public string SemesterName { get; set; }
    
    [ForeignKey("SemesterName")]
    public SemesterEntity Semester { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string GroupNumber { get; set; }
    
    [ForeignKey("GroupNumber")]
    public GroupEntity Group { get; set; }
    
    [Required]
    public double TotalPoints { get; set; }

    [DefaultValue(0)]
    public int Visits { get; set; }
}