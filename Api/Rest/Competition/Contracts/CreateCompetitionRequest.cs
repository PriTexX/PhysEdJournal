using FluentValidation;

namespace Api.Rest.Competition.Contracts;

public sealed class CreateCompetitionRequest
{
    public required string CompetitionName { get; init; }

    public static IValidator<CreateCompetitionRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<CreateCompetitionRequest>
{
    public Validator()
    {
        RuleFor(req => req.CompetitionName)
            .NotEmpty()
            .WithMessage("Название соревнования не может быть пустым");
    }
}
