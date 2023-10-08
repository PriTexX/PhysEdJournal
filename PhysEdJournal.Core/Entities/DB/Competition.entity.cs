using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class CompetitionEntity
{
    [Key]
    [StringLength(255)]
    public string CompetitionName { get; set; }
}
