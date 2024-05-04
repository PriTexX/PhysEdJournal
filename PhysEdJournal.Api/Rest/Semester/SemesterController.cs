using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Common.Filters;
using PhysEdJournal.Api.Rest.Semester.Contracts;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;

namespace PhysEdJournal.Api.Rest.Semester;

public static class SemesterController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(SemesterErrors.Errors);

        router
            .MapPost("/semester", StartNewSemester)
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);
    }

    private static async Task<IResult> StartNewSemester(
        string semesterName,
        [FromServices] StartNewSemesterCommand startNewSemesterCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var res = await startNewSemesterCommand.ExecuteAsync(semesterName);

        return res.Match(Response.Ok, Response.Error);
    }
}
