using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<StandardsStudentHistoryEntity>
{
    public Validator()
    {
        RuleFor(g => g.TeacherGuid).NotEmpty();
        RuleFor(g => g.Date).NotEmpty();
        RuleFor(g => g.StudentGuid).NotEmpty();
        RuleFor(g => g.Points).GreaterThan(0).LessThanOrEqualTo(50);
    }
}

public static class StandardsHistory
{
    public static IValidator<StandardsStudentHistoryEntity> Validator => new Validator();
}
