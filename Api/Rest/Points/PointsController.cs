using Api.Rest.Common;
using Api.Rest.Common.Filters;
using Api.Rest.Points.Contracts;
using Core.Commands;
using DB.Tables;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest;

public static class PointsController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(PointsErrors.Errors);

        var pointsRouter = router.MapGroup("student/points");

        pointsRouter
            .MapPost("/other", AddPointsToStudent)
            .AddValidation(AddPointsToStudentRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapPost("/standard", AddPointsForStandardToStudent)
            .AddValidation(AddPointsForStandardToStudentRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapPost("/visit", IncreaseStudentVisits)
            .AddValidation(IncreaseStudentVisitsRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/visit", DeleteStudentVisit)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/other", DeletePoints)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/standard", DeleteStandardPoints)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();
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
                    TeacherPermissions.SecretaryAccess
                );
                break;
            }

            case WorkType.OnlineWork:
            {
                await permissionValidator.ValidateTeacherPermissionsAndThrow(
                    callerGuid,
                    TeacherPermissions.OnlineCourseAccess
                );
                break;
            }
        }

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsPayload = new AddPointsPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.Points,
            Date = request.Date,
            WorkType = request.WorkType,
            IsAdminOrSecretary = isAdminOrSecretary,
            Comment = request.Comment,
        };

        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> AddPointsForStandardToStudent(
        [FromBody] AddPointsForStandardToStudentRequest request,
        [FromServices] AddStandardCommand addStandardPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsForStandardPayload = new AddStandardPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.Points,
            Date = request.Date,
            StandardType = request.StandardType,
            IsAdminOrSecretary = isAdminOrSecretary,
            Comment = request.Comment,
        };
        var res = await addStandardPointsCommand.ExecuteAsync(addPointsForStandardPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> IncreaseStudentVisits(
        [FromBody] IncreaseStudentVisitsRequest request,
        [FromServices] AddVisitCommand increaseStudentVisitsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var increaseStudentVisitsPayload = new AddVisitPayload
        {
            Date = request.Date,
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            IsAdminOrSecretary = isAdminOrSecretary,
        };

        var res = await increaseStudentVisitsCommand.ExecuteAsync(increaseStudentVisitsPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeleteStudentVisit(
        int historyId,
        [FromServices] DeleteVisitCommand deleteStudentVisitCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStudentVisitCommand.ExecuteAsync(
            new DeleteVisitPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeletePoints(
        int historyId,
        [FromServices] DeletePointsCommand deletePointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deletePointsCommand.ExecuteAsync(
            new DeletePointsPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeleteStandardPoints(
        int historyId,
        [FromServices] DeleteStandardCommand deleteStandardPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
        );

        var isAdmin = validationResult.IsOk;

        var res = await deleteStandardPointsCommand.ExecuteAsync(
            new DeleteStandardPayload
            {
                HistoryId = historyId,
                IsAdminOrSecretary = isAdmin,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }
}
