﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class StudentEntity
{
    [StringLength(36)]
    [Key]
    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }

    [StringLength(120)]
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }

    [StringLength(20)]
    [Required(AllowEmptyStrings = false)]
    public string GroupNumber { get; set; }

    [DefaultValue(false)]
    public bool HasDebtFromPreviousSemester { get; set; } // Если студент не получил зачет из-за нехватки баллов

    [DefaultValue(0)]
    public double ArchivedVisitValue { get; set; } // Сохраняем стоимость посещения в прошлом семестре здесь, чтобы считать по ней баллы, пока не набертся 50

    // После набора 50 баллов и получения зачета вернуть в Null, а HasDebt в false
    [DefaultValue(0)]
    public int AdditionalPoints { get; set; }

    [DefaultValue(0)]
    [Range(0, 30)]
    public int PointsForStandards { get; set; }

    [ForeignKey("GroupNumber")]
    public GroupEntity? Group { get; set; }

    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    [DefaultValue(0)]
    public int Visits { get; set; }

    [Required]
    [Range(1, 6)]
    public int Course { get; set; }

    [StringLength(32)]
    [Required(AllowEmptyStrings = false)]
    public string CurrentSemesterName { get; set; }

    [ForeignKey("CurrentSemesterName")]
    public SemesterEntity Semester { get; set; }

    [DefaultValue(HealthGroupType.None)]
    public HealthGroupType HealthGroup { get; set; }

    [StringLength(200)]
    public string? Department { get; set; }

    public ICollection<PointsStudentHistoryEntity> PointsStudentHistory { get; set; }

    public ICollection<VisitStudentHistoryEntity> VisitsStudentHistory { get; set; }

    public ICollection<StandardsStudentHistoryEntity> StandardsStudentHistory { get; set; }
}
