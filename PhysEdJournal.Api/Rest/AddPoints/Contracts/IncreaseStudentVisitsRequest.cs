using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

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
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.Date)
                .NotEmpty()
                .WithMessage("Дата должна быть действительной");
        }
    }
}
