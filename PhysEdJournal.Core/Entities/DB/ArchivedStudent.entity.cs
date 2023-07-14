using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysEdJournal.Core.Entities.DB;

public sealed class ArchivedStudentEntity
{
    [Required(AllowEmptyStrings = false)]
    public string StudentGuid { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]
    public string SemesterName { get; set; }
    
    [ForeignKey("SemesterName")]
    public SemesterEntity Semester { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string FullName { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string GroupNumber { get; set; }
    
    [ForeignKey("GroupNumber")]
    public GroupEntity Group { get; set; }
    
    [Required]
    public double TotalPoints { get; set; }

    [DefaultValue(0)]
    public int Visits { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is null || this.GetType() != obj.GetType())
        {
            return false;
        }

        var other = (ArchivedStudentEntity)obj;

        return StudentGuid == other.StudentGuid &&
               SemesterName == other.SemesterName &&
               FullName == other.FullName &&
               GroupNumber == other.GroupNumber &&
               TotalPoints == other.TotalPoints &&
               Visits == other.Visits;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + StudentGuid.GetHashCode();
            hash = hash * 23 + SemesterName.GetHashCode();
            hash = hash * 23 + FullName.GetHashCode();
            hash = hash * 23 + GroupNumber.GetHashCode();
            hash = hash * 23 + TotalPoints.GetHashCode();
            hash = hash * 23 + Visits.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(ArchivedStudentEntity a, ArchivedStudentEntity b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ArchivedStudentEntity a, ArchivedStudentEntity b)
    {
        return !(a == b);
    }
}