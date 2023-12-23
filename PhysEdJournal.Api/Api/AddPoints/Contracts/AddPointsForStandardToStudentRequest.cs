using FluentValidation;
using PhysEdJournal.Core.Entities.Types;
using static PhysEdJournal.Core.Constants.ModelsConstants;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Api.Api.AddPoints.Contracts;

public class AddPointsForStandardToStudentRequest
{
    public required string StudentGuid { get; init; }
    public required int PointsAmount { get; init; }
    public required DateOnly Date { get; init; }
    public required StandardType StandardType { get; init; }
    public required bool IsOverride { get; init; }
    public string? Comment { get; init; } = null;

    public sealed class Validator : AbstractValidator<AddPointsForStandardToStudentRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid).Length(GuidLength, GuidLength).NotEmpty();
            RuleFor(request => request.PointsAmount)
                .NotEmpty()
                .GreaterThan(0)
                .LessThan(MAX_POINTS_FOR_ONE_STANDARD);
            RuleFor(request => request.Date).NotEmpty();
            RuleFor(request => request.StandardType).NotEmpty();
            RuleFor(request => request.IsOverride).NotEmpty();
        }
    }
}
