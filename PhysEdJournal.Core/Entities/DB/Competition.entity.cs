using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class CompetitionEntity
{
    [StringLength(100)]
    [Key]
    public string CompetitionName { get; set; }
}