using FluentValidation;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Api.Teacher.Contracts;

public sealed class CreateTeacherRequest
{
    public required string TeacherGuid { get; init; }
    public required string FullName { get; init; }

    public sealed class Validator : AbstractValidator<CreateTeacherRequest>
    {
        public Validator()
        {
            RuleFor(request => request.TeacherGuid)
                .NotEmpty()
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле должно содержать гуид стандартного формата");
            RuleFor(request => request.FullName)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым")
                .MaximumLength(120)
                .WithMessage("Длина ФИО должна быть меньше");
        }
    }
}
