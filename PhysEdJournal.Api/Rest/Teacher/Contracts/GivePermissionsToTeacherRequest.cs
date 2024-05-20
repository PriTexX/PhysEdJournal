using FluentValidation;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.Rest.Teacher.Contracts;

public class GivePermissionsToTeacherRequest
{
    public required string TeacherGuid { get; init; }

    public required List<TeacherPermissions> Permissions { get; init; }

    public static IValidator<GivePermissionsToTeacherRequest> GetValidator()
    {
        return new Validator();
    }
}

file sealed class Validator : AbstractValidator<GivePermissionsToTeacherRequest>
{
    public Validator()
    {
        RuleFor(request => request.TeacherGuid)
            .NotEmpty()
            .WithMessage("В поле должен передаваться гуид");

        RuleFor(request => request.Permissions)
            .NotEmpty()
            .WithMessage("Нужно передать список прав для учителя");
    }
}
