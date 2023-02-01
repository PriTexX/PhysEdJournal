using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public class StudentEntity
{
    [Required] 
    public string FullName { get; set; }
    
    [Required]
    public string Group { get; set; }
    
    [Required]
    [Range(1, 6)]
    public int Course { get; set; }
    
    public int? HealthGroup { get; set; }
    
    public string? Department { get; set; } 
}