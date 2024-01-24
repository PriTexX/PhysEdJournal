using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Api.Archive.Contracts;

public sealed class UnArchiveStudentRequest
{
    public required string StudentGuid { get; init; }
    public required string SemesterName { get; init; }

    public sealed class Validator : AbstractValidator<UnArchiveStudentRequest>
    {
        public Validator()
        {
            RuleFor(request => request.StudentGuid)
                .Length(GuidLength, GuidLength)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым");
            RuleFor(request => request.SemesterName)
                .Matches(@"\d{4}-\d{4}/\w{5}")
                .NotEmpty() // 2022-2023/весна
                .WithMessage(
                    "Имя семестра должно соответствовать паттерну - yearStart-yearEnd/monthName"
                );
        }
    }
}
