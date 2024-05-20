using FluentValidation;

namespace PhysEdJournal.Api.Rest.Student.Contracts;

public sealed class ArchiveStudentRequest
{
    public required string StudentGuid { get; init; }

    public required string SemesterName { get; init; }

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

        RuleFor(request => request.SemesterName)
            .NotEmpty()
            .Matches(@"\d{4}-\d{4}/\w{5}") // 2022-2023/весна
            .WithMessage("Название семестра должно соответствовать шаблону 2022-2023/весна");
    }
}
