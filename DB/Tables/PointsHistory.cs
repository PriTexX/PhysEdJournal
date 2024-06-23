using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HotChocolate;

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

[GraphQLName("PointsStudentHistoryEntity")]
[Table("PointsHistory")]
public sealed class PointsHistoryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "date")]
    public required DateOnly Date { get; set; }

    public required int Points { get; set; }

    [StringLength(36)]
    public required string TeacherGuid { get; set; }

    [ForeignKey("TeacherGuid")]
    public TeacherEntity? Teacher { get; set; }

    public required WorkType WorkType { get; set; }

    public string? Comment { get; set; }

    [StringLength(36)]
    public required string StudentGuid { get; set; }

    [ForeignKey("StudentGuid")]
    public StudentEntity? Student { get; set; }
}
