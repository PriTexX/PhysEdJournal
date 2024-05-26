using FluentValidation;
using PhysEdJournal.Core.Entities.DB;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<TeacherEntity>
{
    public Validator()
    {
        RuleFor(g => g.FullName).NotEmpty();
        RuleFor(g => g.TeacherGuid).NotEmpty();
    }
}

public static class Teacher
{
    public static IValidator<TeacherEntity> Validator => new Validator();
}
