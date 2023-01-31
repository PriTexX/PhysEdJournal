using System.ComponentModel.DataAnnotations;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DTO;

public record struct PointsDto
{
    [Required]
    public int Value { get; init; }
    
    [Required]
    public DateOnly WorkDate { get; init; }
    
    public SportType SportType { get; init; }
    
    public WorkType WorkType { get; init; }
    
    public string? Comment { get; init; }
}