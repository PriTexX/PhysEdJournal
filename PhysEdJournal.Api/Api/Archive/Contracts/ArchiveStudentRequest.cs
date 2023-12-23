using FluentValidation;

namespace PhysEdJournal.Api.Api.Archive.Contracts;

public sealed class ArchiveStudentRequest
{
    public required string StudentGuid { get; init; }
    public required bool IsForceMode { get; init; } = false;

    public sealed class Validator : AbstractValidator<ArchiveStudentRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid).NotEmpty();
        }
    }
}
