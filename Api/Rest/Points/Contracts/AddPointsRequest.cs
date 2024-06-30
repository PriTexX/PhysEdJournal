using Core.Config;
using DB.Tables;
using FluentValidation;

namespace Api.Rest.Points.Contracts;

public class AddPointsRequest
{
    public required string StudentGuid { get; init; }

    public required int Points { get; init; }

    public required DateOnly Date { get; init; }

    public required WorkType Type { get; init; }

    public string? Comment { get; init; }

    public static IValidator<AddPointsRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AddPointsRequest>
{
    public Validator()
    {
        RuleFor(req => req.StudentGuid).NotEmpty().WithMessage("В поле должен передаваться гуид");

        RuleFor(req => req.Points)
            .GreaterThan(0)
            .WithMessage("Количество баллов должно быть больше 0")
            .LessThanOrEqualTo(Cfg.MaxPointsAmount)
            .WithMessage($"Количество баллов должно быть меньше {Cfg.MaxPointsAmount}");

        RuleFor(req => req.Date).NotEmpty().WithMessage("Дата не может быть пустой");

        RuleFor(req => req.Type).NotNull().WithMessage("Тип работ не может быть пустым");
    }
}
