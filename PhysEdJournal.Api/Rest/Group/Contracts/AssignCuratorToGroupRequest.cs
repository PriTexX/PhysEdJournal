using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Rest.Group.Contracts;

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
                );
            RuleFor(request => request.TeacherGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("В поле должен передаваться гуид стандартного формата");
        }
    }
}
