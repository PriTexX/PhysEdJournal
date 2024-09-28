using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HotChocolate;

namespace DB.Tables;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthGroupType
{
    None,
    Basic,
    Preparatory,
    SpecialA,
    SpecialB,
    HealthLimitations,
    Disabled,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpecializationType
{
    None,
    Basketball,
    Volleyball,
    Aerobics,
    PowerLiftingAndCrossfit,
    StreetLiftingAndArmLifting,
    GeneralPhysicalTraining, // ОФП
    GeneralPhysicalTrainingGym,
    FootRoom,
    SMG,
    TableTennis,
    NordicWalking,
}

[Table("Students")]
public sealed class StudentEntity
{
    [Key]
    [StringLength(36)]
    public required string StudentGuid { get; set; }

    [StringLength(120)]
    public required string FullName { get; set; }

    [StringLength(20)]
    public required string GroupNumber { get; set; }

    [GraphQLName("hasDebtFromPreviousSemester")]
    public bool HasDebt { get; set; } // Если студент не получил зачет из-за нехватки баллов

    public bool HadDebtInSemester { get; set; } // Если у студента был долг в семестре

    public double ArchivedVisitValue { get; set; } // Сохраняем стоимость посещения в прошлом семестре здесь, чтобы считать по ней баллы, пока не набертся 50

    // После набора 50 баллов и получения зачета вернуть в Null, а HasDebt в false
    public int AdditionalPoints { get; set; }

    public int PointsForStandards { get; set; }

    [ForeignKey("GroupNumber")]
    public GroupEntity? Group { get; set; }

    public bool IsActive { get; set; } = true;

    public int Visits { get; set; }

    public int Course { get; set; }

    [StringLength(32)]
    public required string CurrentSemesterName { get; set; }

    [ForeignKey("CurrentSemesterName")]
    public SemesterEntity? Semester { get; set; }

    public HealthGroupType HealthGroup { get; set; }
    
    public TeacherEntity? HealthGroupProvider { get; set; }
    
    public SpecializationType Specialization { get; set; }

    [StringLength(200)]
    public string? Department { get; set; }

    [Timestamp]
    [GraphQLIgnore]
    public uint Version { get; set; }

    [GraphQLName("PointsStudentHistory")]
    public ICollection<PointsHistoryEntity>? PointsHistory { get; set; }

    [GraphQLName("VisitsStudentHistory")]
    public ICollection<VisitsHistoryEntity>? VisitsHistory { get; set; }

    [GraphQLName("StandardsStudentHistory")]
    public ICollection<StandardsHistoryEntity>? StandardsHistory { get; set; }
}
