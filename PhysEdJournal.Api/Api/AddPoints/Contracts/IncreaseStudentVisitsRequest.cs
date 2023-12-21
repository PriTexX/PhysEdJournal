using FluentValidation;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public sealed class IncreaseStudentVisitsRequest
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }

    public sealed class Validator : AbstractValidator<IncreaseStudentVisitsRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid).NotEmpty();
            RuleFor(request => request.Date).NotEmpty();
        }
    }
}
