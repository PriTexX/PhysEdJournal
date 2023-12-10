using FluentValidation;

namespace PhysEdJournal.Api.Api.Group.Contracts;

public sealed class AssignVisitValueRequest
{
    public required string GroupName { get; init; }

    public required double NewVisitValue { get; init; }
}

public sealed class AssignVisitValueValidator : AbstractValidator<AssignVisitValueRequest>
{
    public AssignVisitValueValidator()
    {
        RuleFor(request => request.GroupName).NotEmpty();
        RuleFor(request => request.NewVisitValue).NotEmpty().GreaterThan(0);
    }
}
