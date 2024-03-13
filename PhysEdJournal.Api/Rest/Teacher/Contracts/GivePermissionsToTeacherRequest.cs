using FluentValidation;
using PhysEdJournal.Core.Entities.Types;
using static PhysEdJournal.Core.Constants.ModelsConstants;

namespace PhysEdJournal.Api.Rest.Teacher.Contracts;

public class GivePermissionsToTeacherRequest
{
    public required string TeacherGuid { get; init; }
    public required IList<TeacherPermissions> Permissions { get; init; }

    public sealed class Validator : AbstractValidator<GivePermissionsToTeacherRequest>
    {
        public Validator()
        {
            RuleFor(request => request.TeacherGuid)
                .Length(GuidLength, GuidLength)
                .WithMessage("Поле должно содержать гуид стандартного формата");
            RuleFor(request => request.Permissions)
                .NotEmpty()
                .WithMessage("Поле не должно быть пустым");
        }
    }
}
