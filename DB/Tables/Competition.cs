using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Tables;

[Table("Competitions")]
public sealed class CompetitionEntity
{
    [Key]
    [StringLength(255)]
    public required string CompetitionName { get; set; }
}
