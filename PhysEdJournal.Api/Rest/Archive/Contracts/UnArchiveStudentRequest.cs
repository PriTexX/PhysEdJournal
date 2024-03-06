using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Rest.Archive.Contracts;

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
