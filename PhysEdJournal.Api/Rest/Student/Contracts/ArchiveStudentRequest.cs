using FluentValidation;

namespace PhysEdJournal.Api.Rest.Student.Contracts;

public sealed class ArchiveStudentRequest
{
    public required string StudentGuid { get; init; }

    public static IValidator<ArchiveStudentRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<ArchiveStudentRequest>
{
    public Validator()
    {
        RuleFor(request => request.StudentGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");
    }
}
