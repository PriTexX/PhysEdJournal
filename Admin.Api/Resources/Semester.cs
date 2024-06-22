using DB.Tables;
using FluentValidation;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<SemesterEntity>
{
    public Validator()
    {
        RuleFor(g => g.Name).NotEmpty();
    }
}

public static class Semester
{
    public static IValidator<SemesterEntity> Validator => new Validator();
}
