using PhysEdJournal.Core.Exceptions.StudentExceptions;

namespace PhysEdJournal.Api.Api.Archive.Contracts;

public static class ArchiveErrors
{
    public static readonly Dictionary<string, Func<Exception, ProblemDetailsResponse>> Errors =
        new()
        {
            {
                nameof(NotEnoughPointsException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "not-enough-points",
                        Title = "Not enough points to archive student",
                        Detail = "Student must have more points to be archived",
                    }
            },
            {
                nameof(CannotMigrateToNewSemesterException),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status409Conflict,
                        Type = "no-semester-migration",
                        Title = "Cannot migrate to new semester",
                        Detail = "Student already has an active semester",
                    }
            },
            {
                nameof(ArchivedStudentNotFound),
                _ =>
                    new ProblemDetailsResponse
                    {
                        Status = StatusCodes.Status404NotFound,
                        Type = "archived-student-not-found",
                        Title = "Archived student not found",
                        Detail = "Archived student was not found in the database",
                    }
            },
        };
}
