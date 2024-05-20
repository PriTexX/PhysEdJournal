using FluentValidation;

namespace PhysEdJournal.Api.Rest.Teacher.Contracts;

public sealed class CreateTeacherRequest
{
    public required string TeacherGuid { get; init; }

    public required string FullName { get; init; }

    public static IValidator<CreateTeacherRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<CreateTeacherRequest>
{
    public Validator()
    {
        RuleFor(request => request.TeacherGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithMessage("Нужно передать ФИО")
            .MaximumLength(120)
            .WithMessage("Длина ФИО должна быть не больше 120 символов");
    }
}
