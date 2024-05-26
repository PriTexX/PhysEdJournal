using FluentValidation;
using PhysEdJournal.Core.Entities.DB;

namespace Admin.Api.Resources;

file sealed class Validator : AbstractValidator<CompetitionEntity>
{
    public Validator()
    {
        RuleFor(g => g.CompetitionName).NotEmpty();
    }
}

public static class Competition
{
    public static IValidator<CompetitionEntity> Validator => new Validator();
}
