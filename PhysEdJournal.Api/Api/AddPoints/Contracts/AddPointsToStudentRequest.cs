using FluentValidation;
using PhysEdJournal.Core.Entities.Types;
using static PhysEdJournal.Core.Constants.ModelsConstants;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public class AddPointsToStudentRequest
{
    public required string StudentGuid { get; init; }
    public required int PointsAmount { get; init; }
    public required DateOnly Date { get; init; }
    public required WorkType WorkType { get; init; }
    public string? Comment { get; init; } = null;

    public sealed class Validator : AbstractValidator<AddPointsToStudentRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid).Length(GuidLength, GuidLength).NotEmpty();
            RuleFor(request => request.PointsAmount)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(MaxPointsAmount);
            RuleFor(request => request.Date).NotEmpty();
            RuleFor(request => request.WorkType).NotEmpty();
        }
    }
}
