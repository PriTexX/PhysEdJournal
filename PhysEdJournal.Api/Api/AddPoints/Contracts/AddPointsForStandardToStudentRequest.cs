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
            RuleFor(request => request.StudentGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.PointsAmount)
                .GreaterThan(0)
                .WithMessage($"Колличество баллов должно быть больше 0")
                .LessThan(MAX_POINTS_FOR_ONE_STANDARD)
                .WithMessage(
                    $"Колличество баллов не должно быть больше {MAX_POINTS_FOR_ONE_STANDARD}"
                );
            RuleFor(request => request.Date)
                .NotEmpty()
                .WithMessage("Дата должна быть действительной");
            RuleFor(request => request.StandardType)
                .NotEmpty()
                .WithMessage("Это поле не должно быть пустым");
            RuleFor(request => request.IsOverride)
                .NotEmpty()
                .WithMessage("Это поле не должно быть пустым");
        }
    }
}
