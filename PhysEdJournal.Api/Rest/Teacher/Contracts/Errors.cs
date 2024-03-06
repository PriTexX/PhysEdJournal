using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest.Teacher.Contracts;

public static class TeacherErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(TeacherAlreadyExistsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "teacher-already-exists",
                        Title = "Такой учитель уже есть в системе",
                        Detail = "Невозможно создать одного и того же учителя дважды",
                    }
            },
            {
                nameof(CannotGrantSuperUserPermissionsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "cannot-give-superuser-permissions",
                        Title = "Невозможно выдать права суперпользователя учителю",
                        Detail = "Этот учитель не может быть наделен правами суперпользователя",
                    }
            },
        };
}