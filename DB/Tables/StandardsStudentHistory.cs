using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Tables;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StandardType
{
    Tilts, // Наклоны
    Jumps, // Прыжки
    PullUps, // Подтягивания
    Squats, // Приседания
    JumpingRopeJumps, // Прыжки через скакалку
    TorsoLifts, // Поднимания туловища
    FlexionAndExtensionOfArms, // Сгибания и разгибания рук
    ShuttleRun, // Челночный бег
    Other,
}

public sealed class StandardsStudentHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Range(2, 10)]
    public int Points { get; set; }

    [Column(TypeName = "date")]
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public StandardType StandardType { get; set; }

    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity? Teacher { get; set; }

    [StringLength(36)]
    [Required(AllowEmptyStrings = false)]
    public required string StudentGuid { get; set; }

    [ForeignKey("StudentGuid")]
    public StudentEntity? Student { get; set; }

    public string? Comment { get; set; }
}
