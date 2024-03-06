using Microsoft.AspNetCore.Mvc;
using PhysEdJournal.Api.Rest.AddPoints.Contracts;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Api.Rest.AddPoints;

public static class AddPointsController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(AddPointsErrors.Errors);

        router
            .MapPost("/AddPointsToStudent", AddPointsToStudent)
            .AddEndpointFilter<ValidationFilter<AddPointsToStudentRequest>>();
        router
            .MapPost("/AddPointsForStandardToStudent", AddPointsForStandardToStudent)
            .AddEndpointFilter<ValidationFilter<AddPointsForStandardToStudentRequest>>();
        router
            .MapPost("/IncreaseStudentVisits", IncreaseStudentVisits)
            .AddEndpointFilter<ValidationFilter<IncreaseStudentVisitsRequest>>();
    }

    private static async Task<IResult> AddPointsToStudent(
        [FromBody] AddPointsToStudentRequest request,
        [FromServices] AddPointsCommand addPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        switch (request.WorkType)
        {
            case WorkType.InternalTeam
            or WorkType.Activist
            or WorkType.Competition:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS
                );
                break;
            }

            case WorkType.OnlineWork:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    ADD_POINTS_FOR_LMS_PERMISSIONS
                );
                break;
            }
        }

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsSuccess;

        var addPointsPayload = new AddPointsCommandPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.PointsAmount,
            Date = request.Date,
            WorkType = request.WorkType,
            IsAdmin = isAdminOrSecretary,
            Comment = request.Comment,
        };

        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> AddPointsForStandardToStudent(
        [FromBody] AddPointsForStandardToStudentRequest request,
        [FromServices] AddStandardPointsCommand addStandardPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsSuccess;

        var addPointsForStandardPayload = new AddStandardPointsCommandPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.PointsAmount,
            Date = request.Date,
            StandardType = request.StandardType,
            IsOverride = request.IsOverride,
            IsAdmin = isAdminOrSecretary,
            Comment = request.Comment,
        };
        var res = await addStandardPointsCommand.ExecuteAsync(addPointsForStandardPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> IncreaseStudentVisits(
        [FromBody] IncreaseStudentVisitsRequest request,
        [FromServices] IncreaseStudentVisitsCommand increaseStudentVisitsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        await permissionValidator.ValidateTeacherPermissionsAndThrow(
            callerGuid,
            TeacherPermissions.DefaultAccess
        );

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            FOR_ONLY_ADMIN_USE_PERMISSIONS | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsSuccess;

        var increaseStudentVisitsPayload = new IncreaseStudentVisitsCommandPayload
        {
            Date = request.Date,
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            IsAdmin = isAdminOrSecretary,
        };

        var res = await increaseStudentVisitsCommand.ExecuteAsync(increaseStudentVisitsPayload);

        return res.Match(Response.Ok, Response.Error);
    }
}