using FluentValidation;
using PhysEdJournal.Core;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.Rest.Points.Contracts;

public class AddPointsForStandardToStudentRequest
{
    public required string StudentGuid { get; init; }

    public required int Points { get; init; }

    public required DateOnly Date { get; init; }

    public required StandardType StandardType { get; init; }

    public required bool IsOverride { get; init; }

    public string? Comment { get; init; } = null;

    public static IValidator<AddPointsForStandardToStudentRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AddPointsForStandardToStudentRequest>
{
    public Validator()
    {
        RuleFor(request => request.StudentGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.Points)
            .GreaterThan(0)
            .WithMessage("Количество баллов должно быть больше 0")
            .LessThanOrEqualTo(Constants.MaxPointsForOneStandard)
            .WithMessage(
                $"Количество баллов не должно быть больше {Constants.MaxPointsForOneStandard}"
            );

        RuleFor(request => request.Date).NotEmpty().WithMessage("Обязательно нужно указать дату");

        RuleFor(request => request.StandardType)
            .NotNull()
            .WithMessage("Обязательно нужно указать тип норматива");

        RuleFor(request => request.IsOverride)
            .NotNull()
            .WithMessage("Обязательно нужно указать пересдается ли норматив");
    }
}
