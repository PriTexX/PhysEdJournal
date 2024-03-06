using FluentValidation;
using PhysEdJournal.Core.Entities.Types;
using static PhysEdJournal.Core.Constants.ModelsConstants;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Api.Rest.AddPoints.Contracts;

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
            RuleFor(request => request.StudentGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.PointsAmount)
                .GreaterThan(0)
                .WithMessage($"Кол-во дополнительных баллов должно быть больше 0")
                .LessThan(MaxPointsAmount)
                .WithMessage(
                    $"Кол-во дополнительных баллов не должно быть больше {MaxPointsAmount}"
                );
            RuleFor(request => request.Date)
                .NotEmpty()
                .WithMessage("Дата должна быть действительной");
            RuleFor(request => request.WorkType)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым");
        }
    }
}
