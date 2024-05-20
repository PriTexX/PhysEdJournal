using FluentValidation;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

public sealed class AssignCuratorToGroupRequest
{
    public required string GroupName { get; init; }

    public required string TeacherGuid { get; init; }

    public static IValidator<AssignCuratorToGroupRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<AssignCuratorToGroupRequest>
{
    public Validator()
    {
        RuleFor(request => request.GroupName)
            .Length(1, 30)
            .WithMessage("Длина названия группы должна быть не меньше 1 и не больше 30 символов");

        RuleFor(request => request.TeacherGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");
    }
}
