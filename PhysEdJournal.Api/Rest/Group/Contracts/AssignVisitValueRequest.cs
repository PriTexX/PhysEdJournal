using FluentValidation;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

public sealed class AssignVisitValueRequest
{
    public required string GroupName { get; init; }

    public required double NewVisitValue { get; init; }

    public static IValidator<AssignVisitValueRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AssignVisitValueRequest>
{
    public Validator()
    {
        RuleFor(request => request.GroupName)
            .Length(1, 30)
            .WithMessage("Длина названия группы должна быть не меньше 1 и не больше 30 символов");

        RuleFor(request => request.NewVisitValue)
            .NotEmpty()
            .WithMessage("Обязательно должна быть передана стоимость посещений")
            .LessThanOrEqualTo(4)
            .WithMessage("Новое значение для стоимости посещения не может быть больше 4");
    }
}
