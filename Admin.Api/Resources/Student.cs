using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<StudentEntity>
{
    public Validator()
    {
        RuleFor(s => s.StudentGuid).NotEmpty();
        RuleFor(s => s.Course).LessThanOrEqualTo(6).GreaterThan(0);
        RuleFor(s => s.Visits).GreaterThanOrEqualTo(0);
        RuleFor(s => s.AdditionalPoints).GreaterThanOrEqualTo(0);
        RuleFor(s => s.FullName).NotEmpty();
        RuleFor(s => s.GroupNumber).NotEmpty();
        RuleFor(s => s.CurrentSemesterName).NotEmpty();
        RuleFor(s => s.PointsForStandards).GreaterThanOrEqualTo(0).LessThanOrEqualTo(30);
    }
}

public static class Student
{
    public static IValidator<StudentEntity> Validator => new Validator();

    public static string[] SortFields =>
        ["FullName", "Course", "Group", "HasDebtFromPreviousSemester", "HadDebtInSemester"];
}
