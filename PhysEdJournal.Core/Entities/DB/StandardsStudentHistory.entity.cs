﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Entities.DB;

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

    [StringLength(32)]
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]
    public required string SemesterName { get; set; }

    [ForeignKey("SemesterName")]
    public SemesterEntity? Semester { get; set; }

    [DefaultValue(false)]
    public bool IsArchived { get; set; }

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

    [NotMapped]
    public bool ShouldBeArchived { get; set; } = true;
}
