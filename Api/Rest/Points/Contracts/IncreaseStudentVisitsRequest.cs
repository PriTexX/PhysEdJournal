using FluentValidation;

namespace Api.Rest.Points.Contracts;

public sealed class IncreaseStudentVisitsRequest
{
    public required string StudentGuid { get; init; }

    public required DateOnly Date { get; init; }

    public static IValidator<IncreaseStudentVisitsRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<IncreaseStudentVisitsRequest>
{
    public Validator()
    {
        RuleFor(request => request.StudentGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.Date).NotEmpty().WithMessage("Обязательно нужно указать дату");
    }
}
