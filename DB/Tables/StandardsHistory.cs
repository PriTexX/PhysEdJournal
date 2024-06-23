using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HotChocolate;

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

[GraphQLName("StandardsStudentHistoryEntity")]
[Table("StandardsHistory")]
public sealed class StandardsHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Range(2, 10)]
    public int Points { get; set; }

    [Column(TypeName = "date")]
    public DateOnly Date { get; set; }

    public StandardType StandardType { get; set; }

    [StringLength(36)]
    public required string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity? Teacher { get; set; }

    [StringLength(36)]
    public required string StudentGuid { get; set; }

    [ForeignKey("StudentGuid")]
    public StudentEntity? Student { get; set; }

    public string? Comment { get; set; }
}
