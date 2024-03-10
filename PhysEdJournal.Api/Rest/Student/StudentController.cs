using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Rest.Student;

public static class StudentController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        router.MapPost("/activate-student", ActivateStudent);
        router.MapPost("/de-activate-student", DeActivateStudent);
        router.MapGet("/get-student", GetStudent);
        router.MapGet("/get-students", GetStudents);
    }

    private static async Task<IResult> ActivateStudent(
        string studentGuid,
        [FromServices] ActivateStudentCommand activateStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await activateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeActivateStudent(
        string studentGuid,
        [FromServices] DeActivateStudentCommand deActivateStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS
        );

        var res = await deActivateStudentCommand.ExecuteAsync(studentGuid);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> GetStudent(
        string guid,
        [FromServices] ApplicationContext context
    )
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.StudentGuid == guid);

        return Response.Ok(student);
    }

    private static IResult GetStudents([FromBody] ApplicationContext context)
    {
        return Response.Ok(context.Students);
    }
}
