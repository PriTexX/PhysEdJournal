using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<PointsHistoryEntity>
{
    public Validator()
    {
        RuleFor(g => g.TeacherGuid).NotEmpty();
        RuleFor(g => g.Date).NotEmpty();
        RuleFor(g => g.StudentGuid).NotEmpty();
        RuleFor(g => g.Points).GreaterThan(0).LessThanOrEqualTo(50);
    }
}

public static class PointsHistory
{
    public static IValidator<PointsHistoryEntity> Validator => new Validator();
}
