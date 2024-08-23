using DB.Tables;
using FluentValidation;

namespace Api.Rest.Student.Contracts;

public sealed class SetHealthGroupRequest
{
    public required string StudentGuid { get; init; }
    public required HealthGroupType HealthGroup { get; init; }

    public static IValidator<SetHealthGroupRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<SetHealthGroupRequest>
{
    public Validator()
    {
        RuleFor(r => r.StudentGuid).NotEmpty().WithMessage("В поле должен передаваться гуид");

        RuleFor(r => r.HealthGroup).NotNull().WithMessage("Группа здоровья не может быть пустой");
    }
}
