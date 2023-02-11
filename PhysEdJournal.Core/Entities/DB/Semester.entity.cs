using System.ComponentModel.DataAnnotations;

namespace PhysEdJournal.Core.Entities.DB;

public class SemesterEntity
{
    [Required]
    public int Id { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"\d{4}-\d{4}/\w{5}")]  // 2022-2023/весна
    public string Name { get; set; }

    public ICollection<ArchivedStudentEntity> ArchivedStudents { get; set; }
    public ICollection<StudentPointsHistoryEntity> StudentPointsHistories { get; set; }
    
    public int ToInt() // 2022-2023/весна
    {
        return NameToInt(Name);
    }

    public static int NameToInt(string semesterName)
    {
        ReadOnlySpan<char> nameAsSpan = semesterName;
        int.TryParse(nameAsSpan[..4], out var firstYear); // 2022
        int.TryParse(nameAsSpan[5..9], out var secondYear); // 2023
        var timeOfYear = nameAsSpan[10..]; // весна

        return firstYear + secondYear + (timeOfYear.SequenceEqual("осень") ? 1 : 0);
    }

    public static SemesterEntity FromString(string name)
    {
        return new SemesterEntity { Id = NameToInt(name), Name = name };
    }

    public static SemesterEntity FromInt(int value)
    {
        var ostatok = value % 2;
        var timeOfYear = ostatok == 1 ? "осень" : "весна";

        var year = (value - ostatok) / 2;
        
        return new SemesterEntity{Id = value, Name = $"{year - 1}-{year}/{timeOfYear}"};
    }
}