using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Api.Group.Contracts;

public sealed class AssignCuratorToGroupRequest
{
    public required string GroupName { get; init; }

    public required string TeacherGuid { get; init; }

    public sealed class Validator : AbstractValidator<AssignCuratorToGroupRequest>
    {
        public Validator()
        {
            RuleFor(request => request.GroupName)
                .Length(1, 30)
                .WithMessage(
                    "Длина названия группы должна быть не меньше 1 и не больше 30 символов"
                )
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.TeacherGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле не должно быть пустым");
        }
    }
}
