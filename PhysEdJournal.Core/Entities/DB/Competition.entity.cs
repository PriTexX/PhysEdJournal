using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class CompetitionEntity
{
    [Key]
    public string CompetitionName { get; set; }
}