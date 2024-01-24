using FluentValidation;

namespace PhysEdJournal.Api.Api.Archive.Contracts;

public sealed class ArchiveStudentRequest
{
    public required string StudentGuid { get; init; }
    public required string SemesterName { get; init; }

    public sealed class Validator : AbstractValidator<ArchiveStudentRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.SemesterName)
                .NotEmpty()
                .Matches(@"\d{4}-\d{4}/\w{5}")
                .WithMessage("Имя семестра должно соответствовать паттерну - month/year");
        }
    }
}
