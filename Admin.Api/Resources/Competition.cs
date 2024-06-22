using DB.Tables;
using FluentValidation;

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
