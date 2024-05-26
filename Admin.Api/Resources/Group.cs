using FluentValidation;
using PhysEdJournal.Core.Entities.DB;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<GroupEntity>
{
    public Validator()
    {
        RuleFor(g => g.GroupName).NotEmpty();
        RuleFor(g => g.VisitValue).GreaterThan(0);
    }
}

public static class Group
{
    public static IValidator<GroupEntity> Validator => new Validator();
}
