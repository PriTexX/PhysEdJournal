using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.Api.Teacher.Contracts;

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
                        Title = "Teacher duplicate was found",
                        Detail = "You cannot create the same teacher twice",
                    }
            },
            {
                nameof(CannotGrantSuperUserPermissionsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "cannot-grand-superuser-permissions",
                        Title = "Cannot grant superuser permissions to teacher",
                        Detail = "Unable to grand permissions",
                    }
            },
        };
}
