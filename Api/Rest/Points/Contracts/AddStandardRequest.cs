using Core.Config;
using DB.Tables;
using FluentValidation;

namespace Api.Rest.Points.Contracts;

public class AddStandardRequest
{
    public required string StudentGuid { get; init; }

    public required int Points { get; init; }

    public required DateOnly Date { get; init; }

    public required StandardType Type { get; init; }

    public string? Comment { get; init; }

    public static IValidator<AddStandardRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AddStandardRequest>
{
    public Validator()
    {
        RuleFor(request => request.StudentGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.Points)
            .GreaterThan(0)
            .WithMessage("Количество баллов должно быть больше 0")
            .LessThanOrEqualTo(Cfg.MaxPointsForOneStandard)
            .WithMessage($"Количество баллов не должно быть больше {Cfg.MaxPointsForOneStandard}");

        RuleFor(request => request.Date).NotEmpty().WithMessage("Дата не может быть пустой");

        RuleFor(request => request.Type)
            .NotNull()
            .WithMessage("Тип норматива не может быть пустым");
    }
}
