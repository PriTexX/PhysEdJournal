using FluentValidation;

namespace Api.Rest.Points.Contracts;

public sealed class AddVisitRequest
{
    public required string StudentGuid { get; init; }

    public required DateOnly Date { get; init; }

    public static IValidator<AddVisitRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AddVisitRequest>
{
    public Validator()
    {
        RuleFor(req => req.StudentGuid).NotEmpty().WithMessage("В поле должен передаваться гуид");

        RuleFor(req => req.Date).NotEmpty().WithMessage("Дата не может быть пустой");
    }
}
