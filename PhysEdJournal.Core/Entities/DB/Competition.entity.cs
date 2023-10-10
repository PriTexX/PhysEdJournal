using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class CompetitionEntity
{
    [Key]
    [StringLength(255)]
    public required string CompetitionName { get; set; }
}
