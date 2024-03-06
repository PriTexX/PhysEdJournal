using FluentValidation;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

public sealed class AssignVisitValueRequest
{
    public required string GroupName { get; init; }

    public required double NewVisitValue { get; init; }

    public sealed class Validator : AbstractValidator<AssignVisitValueRequest>
    {
        public Validator()
        {
            RuleFor(request => request.GroupName)
                .Length(1, 30)
                .WithMessage(
                    "Длина названия группы должна быть не меньше 1 и не больше 30 символов"
                )
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.NewVisitValue)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым")
                .LessThanOrEqualTo(4)
                .WithMessage("Новое значение для аосещений не может быть больше 4");
        }
    }
}
