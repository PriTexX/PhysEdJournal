using FluentValidation;
using PhysEdJournal.Core;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.Rest.Points.Contracts;

public class AddPointsToStudentRequest
{
    public required string StudentGuid { get; init; }

    public required int Points { get; init; }

    public required DateOnly Date { get; init; }

    public required WorkType WorkType { get; init; }

    public string? Comment { get; init; } = null;

    public static IValidator<AddPointsToStudentRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AddPointsToStudentRequest>
{
    public Validator()
    {
        RuleFor(request => request.StudentGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.Points)
            .GreaterThan(0)
            .WithMessage("Количество баллов должно быть больше 0")
            .LessThan(Constants.MaxPointsAmount)
            .WithMessage($"Количество баллов должно быть меньше {Constants.MaxPointsAmount}");

        RuleFor(request => request.Date).NotEmpty().WithMessage("Обязательно нужно указать дату");

        RuleFor(request => request.WorkType)
            .NotEmpty()
            .WithMessage("Обязательно нужно передать тип работы, за которую выставляются баллы");
    }
}
