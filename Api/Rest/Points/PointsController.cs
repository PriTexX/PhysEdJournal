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

        var pointsRouter = router.MapGroup("points");

        pointsRouter
            .MapPost("/other", AddPoints)
            .AddValidation(AddPointsRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapPost("/competitions", AddManyCompetitionPoints)
            .AddPermissionsValidation(
                TeacherPermissions.SecretaryAccess | TeacherPermissions.CompetitionAccess
            )
            .DisableAntiforgery()
            .RequireAuthorization();

        pointsRouter
            .MapPost("/standard", AddStandard)
            .AddValidation(AddStandardRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapPost("/visit", AddVisit)
            .AddValidation(AddVisitRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/visit/{id:int}", DeleteVisit)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/other/{id:int}", DeletePoints)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        pointsRouter
            .MapDelete("/standard/{id:int}", DeleteStandard)
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();
    }

    private static async Task<IResult> AddPoints(
        [FromBody] AddPointsRequest request,
        [FromServices] AddPointsCommand addPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        switch (request.Type)
        {
            case WorkType.Competition:
            {
                var validationResult = await permissionValidator.ValidateTeacherPermissions(
                    callerGuid,
                    TeacherPermissions.SecretaryAccess | TeacherPermissions.CompetitionAccess
                );

                if (validationResult.IsErr)
                {
                    return Response.Error(new NotEnoughPermissionsError());
                }

                break;
            }

            case WorkType.InternalTeam
            or WorkType.Activist:
            {
                var validationResult = await permissionValidator.ValidateTeacherPermissions(
                    callerGuid,
                    TeacherPermissions.SecretaryAccess
                );

                if (validationResult.IsErr)
                {
                    return Response.Error(new NotEnoughPermissionsError());
                }

                break;
            }

            case WorkType.OnlineWork:
            {
                var validationResult = await permissionValidator.ValidateTeacherPermissions(
                    callerGuid,
                    TeacherPermissions.OnlineCourseAccess
                );

                if (validationResult.IsErr)
                {
                    return Response.Error(new NotEnoughPermissionsError());
                }

                break;
            }

            case WorkType.Science:
            {
                var validationResult = await permissionValidator.ValidateTeacherPermissions(
                    callerGuid,
                    TeacherPermissions.ScienceAccess
                );

                if (validationResult.IsErr)
                {
                    return Response.Error(new NotEnoughPermissionsError());
                }

                break;
            }

            case WorkType.GTO:
            {
                var validationResult = await permissionValidator.ValidateTeacherPermissions(
                    callerGuid,
                    TeacherPermissions.GTOAccess
                );

                if (validationResult.IsErr)
                {
                    return Response.Error(new NotEnoughPermissionsError());
                }

                break;
            }
        }

        var validateTeacherPermissionsResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.SecretaryAccess | TeacherPermissions.AdminAccess
        );

        var isAdminOrSecretary = validateTeacherPermissionsResult.IsOk;

        var addPointsPayload = new AddPointsPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = callerGuid,
            Points = request.Points,
            Date = request.Date,
            WorkType = request.Type,
            IsAdminOrSecretary = isAdminOrSecretary,
            Comment = request.Comment,
        };

        var res = await addPointsCommand.ExecuteAsync(addPointsPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> AddManyCompetitionPoints(
        [FromForm] string competition,
        [FromForm] DateOnly date,
        [FromForm] IFormFile file,
        [FromServices] AddManyCompetitionPointsCommand addManyCompetitionPointsCommand,
        HttpContext ctx
    )
    {
        if (Path.GetExtension(file.FileName) != ".csv")
        {
            return Response.Error(
                new ErrorResponse
                {
                    Type = "FileExt",
                    Detail = "Поддерживаются только файлы с расширением `.csv`",
                    StatusCode = 400,
                }
            );
        }

        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var res = await addManyCompetitionPointsCommand.ExecuteAsync(
            new AddManyCompetitionPointsPayload
            {
                Competition = competition,
                Date = date,
                File = file.OpenReadStream(),
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(
            Response.Ok,
            (err) =>
            {
                if (err is not AddManyCompetitionsError compError)
                {
                    throw err;
                }

                var detail = new
                {
                    studentsWithCollisions = compError.StudentsWithNameCollisions,
                    notFoundStudents = compError.NotFoundStudents,
                };

                return Response.Error(
                    new ErrorResponse
                    {
                        Type = "ConstraintViolation",
                        Detail = detail,
                        StatusCode = 400,
                    }
                );
            }
        );
    }

    private static async Task<IResult> AddStandard(
        [FromBody] AddStandardRequest request,
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
            StandardType = request.Type,
            IsAdminOrSecretary = isAdminOrSecretary,
            Comment = request.Comment,
        };
        var res = await addStandardPointsCommand.ExecuteAsync(addPointsForStandardPayload);

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> AddVisit(
        [FromBody] AddVisitRequest request,
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

    private static async Task<IResult> DeleteVisit(
        int id,
        [FromServices] DeleteVisitCommand deleteStudentVisitCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validationResult.IsOk;

        var res = await deleteStudentVisitCommand.ExecuteAsync(
            new DeleteVisitPayload
            {
                HistoryId = id,
                IsAdminOrSecretary = isAdminOrSecretary,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeletePoints(
        int id,
        [FromServices] DeletePointsCommand deletePointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess
                | TeacherPermissions.SecretaryAccess
                | TeacherPermissions.CompetitionAccess
        );

        var isAdminOrSecretary = validationResult.IsOk;

        var res = await deletePointsCommand.ExecuteAsync(
            new DeletePointsPayload
            {
                HistoryId = id,
                IsAdminOrSecretary = isAdminOrSecretary,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> DeleteStandard(
        int id,
        [FromServices] DeleteStandardCommand deleteStandardPointsCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var callerGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var validationResult = await permissionValidator.ValidateTeacherPermissions(
            callerGuid,
            TeacherPermissions.AdminAccess | TeacherPermissions.SecretaryAccess
        );

        var isAdminOrSecretary = validationResult.IsOk;

        var res = await deleteStandardPointsCommand.ExecuteAsync(
            new DeleteStandardPayload
            {
                HistoryId = id,
                IsAdminOrSecretary = isAdminOrSecretary,
                TeacherGuid = callerGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }
}
