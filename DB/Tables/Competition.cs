using System.ComponentModel.DataAnnotations;

namespace DB.Tables;

public sealed class CompetitionEntity
{
    [Key]
    [StringLength(255)]
    public required string CompetitionName { get; set; }
}
