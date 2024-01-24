using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

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
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.SemesterName)
                .NotEmpty()
                .Matches(@"\d{4}-\d{4}/\w{5}") // 2022-2023/весна
                .WithMessage(
                    "Имя семестра должно соответствовать паттерну - yearStart-yearEnd/monthName"
                );
        }
    }
}
