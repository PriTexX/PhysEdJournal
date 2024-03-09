using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Rest.AddPoints.Contracts;

public sealed class IncreaseStudentVisitsRequest
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }

    public sealed class Validator : AbstractValidator<IncreaseStudentVisitsRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("В поле должен передаваться гуид стандартного формата");
            RuleFor(request => request.Date)
                .NotEmpty()
                .WithMessage("Дата должна быть действительной");
        }
    }
}
