using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<StandardsHistoryEntity>
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
    public static IValidator<StandardsHistoryEntity> Validator => new Validator();
}
