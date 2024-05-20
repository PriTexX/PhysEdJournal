using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Rest.Teacher.Contracts;

public static class TeacherErrors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errors =
        new()
        {
            {
                nameof(TeacherAlreadyExistsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Type = "teacher-already-exists",
                    Detail = "Преподаватель уже существует в системе",
                }
            },
            {
                nameof(CannotGrantSuperUserPermissionsException),
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Type = "cannot-give-superuser-permissions",
                    Detail = "Нельзя выдать права суперпользователя",
                }
            },
        };
}
