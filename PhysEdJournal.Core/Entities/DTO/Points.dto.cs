using System.ComponentModel.DataAnnotations;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DTO;

public record struct PointsDto
{
    [Required]
    public int Value { get; set; }
    
    [Required]
    public DateOnly WorkDate { get; set; }
    
    public SportType SportType { get; set; }
    
    public WorkType WorkType { get; set; }
    
    public string? Comment { get; set; }
}