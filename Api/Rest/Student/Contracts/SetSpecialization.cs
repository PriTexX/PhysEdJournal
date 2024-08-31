using DB.Tables;
using FluentValidation;

namespace Api.Rest.Student.Contracts;

public sealed class SetSpecializationRequest
{
    public required string StudentGuid { get; init; }
    public required SpecializationType Specialization { get; init; }

    public static IValidator<SetSpecializationRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<SetSpecializationRequest>
{
    public Validator()
    {
        RuleFor(r => r.StudentGuid).NotEmpty().WithMessage("В поле должен передаваться гуид");

        RuleFor(r => r.Specialization).NotNull().WithMessage("Специализация не может быть пустой");
    }
}
