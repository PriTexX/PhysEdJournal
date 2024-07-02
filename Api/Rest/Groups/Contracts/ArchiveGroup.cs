using Api.Rest.Common;
using FluentValidation;

namespace Api.Rest.Groups.Contracts;

public sealed class ArchiveGroupRequest
{
    public required string GroupName { get; init; }

    public static IValidator<ArchiveGroupRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<ArchiveGroupRequest>
{
    public Validator()
    {
        RuleFor(request => request.GroupName)
            .NotEmpty()
            .WithMessage("В поле должно передаваться название группы");
    }
}

public sealed class ArchiveGroupResponse
{
    public required bool IsArchived { get; set; }

    public required string Guid { get; set; }

    public required string FullName { get; set; }

    public ErrorResponse? Error { get; set; }
}
