using FluentValidation;

namespace PhysEdJournal.Api.Api.Group.Contracts;

public sealed class AssignCuratorToGroupRequest
{
    public required string GroupName { get; init; }

    public required string TeacherGuid { get; init; }

    public sealed class Validator : AbstractValidator<AssignCuratorToGroupRequest>
    {
        public Validator()
        {
            RuleFor(request => request.GroupName).NotEmpty();
            RuleFor(request => request.TeacherGuid).NotEmpty();
        }
    }
}
