using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<VisitsHistoryEntity>
{
    public Validator()
    {
        RuleFor(g => g.TeacherGuid).NotEmpty();
        RuleFor(g => g.Date).NotEmpty();
        RuleFor(g => g.StudentGuid).NotEmpty();
    }
}

public static class VisitsHistory
{
    public static IValidator<VisitsHistoryEntity> Validator => new Validator();
}
