using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Tables;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkType
{
    ExternalFitness,
    GTO,
    Science,
    OnlineWork, // СДО
    InternalTeam, // Сборная
    Activist,
    Competition,
}

public sealed class PointsStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "date")]
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public int Points { get; set; }

    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity? Teacher { get; set; }

    [Required]
    public WorkType WorkType { get; set; }

    public string? Comment { get; set; }

    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string StudentGuid { get; set; }

    [ForeignKey("StudentGuid")]
    public StudentEntity? Student { get; set; }
}
