using Api.Rest.Common;
using Api.Rest.Common.Filters;
using Api.Rest.Student.Contracts;
using Core.Commands;
using Core.Config;
using DB;
using DB.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PResult;
using StudentResponse = Api.Rest.Student.Contracts.StudentResponse;

namespace Api.Rest;

public static class StudentController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(StudentErrors.Errors);

        var studentRouter = router.MapGroup("student");

        studentRouter
            .MapPost("/archive", ArchiveStudent)
            .AddValidation(ArchiveStudentRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        studentRouter
            .MapPost("/health-group", SetHealthGroup)
            .AddValidation(SetHealthGroupRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        studentRouter
            .MapPost("/specialization", SetSpecialization)
            .AddValidation(SetSpecializationRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.DefaultAccess)
            .RequireAuthorization();

        studentRouter.MapGet("/{guid}", GetStudent);

        studentRouter.MapGet("/", GetStudents);
    }

    private static async Task<IResult> GetStudent(
        string guid,
        [FromServices] ApplicationContext appCtx
    )
    {
        var student = await appCtx
            .Students.Where(s => s.StudentGuid == guid)
            .Include(s => s.HealthGroupTeacher)
            .Include(s => s.VisitsHistory!)
            .ThenInclude(h => h.Teacher)
            .Include(s => s.PointsHistory!)
            .ThenInclude(h => h.Teacher)
            .Include(s => s.StandardsHistory!)
            .ThenInclude(h => h.Teacher)
            .Select(s => new
            {
                s.StudentGuid,
                s.FullName,
                s.GroupNumber,
                s.Group!.VisitValue,
                s.HasDebt,
                s.HadDebtInSemester,
                s.Course,
                s.Visits,
                s.AdditionalPoints,
                s.PointsForStandards,
                s.ArchivedVisitValue,
                s.HealthGroup,
                s.HealthGroupTeacher,
                s.Specialization,
                s.PointsHistory,
                s.StandardsHistory,
                s.VisitsHistory,
                s.Group.Curator,
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return Response.Error(new StudentNotFoundError());
        }

        var studentResponse = new GetStudentResponse
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            HasDebt = student.HasDebt,
            HadDebtInSemester = student.HadDebtInSemester,
            Course = student.Course,
            HealthGroup = student.HealthGroup,
            Specialization = student.Specialization,
            Curator = student.Curator is not null
                ? new TeacherResponse
                {
                    Guid = student.Curator.TeacherGuid,
                    FullName = student.Curator.FullName,
                }
                : null,
            HealthGroupTeacher = student.HealthGroupTeacher is not null
                ? new TeacherResponse
                {
                    Guid = student.HealthGroupTeacher.TeacherGuid,
                    FullName = student.HealthGroupTeacher.FullName,
                }
                : null,

            PointsHistory =
                student
                    .PointsHistory?.OrderByDescending(h => h.Date)
                    .Select(h => new PointsHistoryResponse
                    {
                        Id = h.Id,
                        Date = h.Date,
                        Points = h.Points,
                        Type = h.WorkType,
                        Comment = h.Comment,
                        TeacherFullName = h.Teacher!.FullName,
                        TeacherGuid = h.TeacherGuid,
                    })
                    .ToList() ?? [],

            VisitsHistory =
                student
                    .VisitsHistory?.OrderByDescending(h => h.Date)
                    .Select(h => new VisitsHistoryResponse()
                    {
                        Id = h.Id,
                        Date = h.Date,
                        TeacherFullName = h.Teacher!.FullName,
                        TeacherGuid = h.TeacherGuid,
                    })
                    .ToList() ?? [],

            StandardsHistory =
                student
                    .StandardsHistory?.OrderByDescending(h => h.Date)
                    .Select(h => new StandardsHistoryResponse()
                    {
                        Id = h.Id,
                        Date = h.Date,
                        Points = h.Points,
                        Type = h.StandardType,
                        Comment = h.Comment,
                        TeacherFullName = h.Teacher!.FullName,
                        TeacherGuid = h.TeacherGuid,
                    })
                    .ToList() ?? [],

            TotalPoints = Cfg.CalculateTotalPoints(
                student.Visits,
                student.HasDebt ? student.ArchivedVisitValue : student.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ),

            LMSPoints =
                student
                    .PointsHistory?.Where(h => h.WorkType == WorkType.OnlineWork)
                    .Sum(h => h.Points) ?? 0,
        };

        return Response.Ok(studentResponse);
    }

    private static async Task<IResult> GetStudents(
        [AsParameters] GetStudentsRequest req,
        [AsParameters] PaginationParameters paginationParameters,
        [FromQuery] string? sortingParams,
        [FromServices] ApplicationContext context
    )
    {
        IQueryable<StudentEntity> query = context.Students;

        query = query.Where(s => s.IsActive == true);

        if (!string.IsNullOrWhiteSpace(req.FullName))
        {
            query = query.Where(s => EF.Functions.ILike(s.FullName, $"%{req.FullName}%"));
        }

        if (!string.IsNullOrWhiteSpace(req.GroupNumber))
        {
            query = query.Where(s => string.Equals(s.GroupNumber, req.GroupNumber));
        }

        if (req.Course is not null && req.Course > 0)
        {
            query = query.Where(s => Equals(s.Course, req.Course));
        }

        var totalCount = await query.CountAsync();

        query = sortingParams is not null
            ? OrderByAddons.ApplySort(query, sortingParams)
            : query.OrderBy(s => s.FullName);

        var data = await query
            .Select(s => new
            {
                s.StudentGuid,
                s.FullName,
                s.GroupNumber,
                s.HasDebt,
                s.Group!.VisitValue,
                s.PointsForStandards,
                s.Visits,
                s.Course,
                s.AdditionalPoints,
                s.ArchivedVisitValue,
                s.PointsHistory,
                s.HealthGroup,
                s.Specialization,
            })
            .Skip((paginationParameters.Page - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        var students = data.Select(s => new StudentResponse
            {
                StudentGuid = s.StudentGuid,
                FullName = s.FullName,
                GroupNumber = s.GroupNumber,
                HasDebt = s.HasDebt,
                HealthGroup = s.HealthGroup,
                Specialization = s.Specialization,
                StandardPoints = s.PointsForStandards,
                Visits = s.Visits,
                TotalPoints = Cfg.CalculateTotalPoints(
                    s.Visits,
                    s.HasDebt ? s.ArchivedVisitValue : s.VisitValue,
                    s.AdditionalPoints,
                    s.PointsForStandards
                ),
                Course = s.Course,
                LMSPoints =
                    s.PointsHistory?.Where(p => p.WorkType == WorkType.OnlineWork)
                        .Sum(p => p.Points) ?? 0,
            })
            .ToList();

        return Response.Ok(
            new GetStudentsResponse { Students = students, TotalCount = totalCount }
        );
    }

    private static async Task<IResult> ArchiveStudent(
        [FromBody] ArchiveStudentRequest request,
        [FromServices] ArchiveStudentCommand archiveStudentCommand,
        [FromServices] PermissionValidator permissionValidator,
        HttpContext ctx
    )
    {
        var userGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var isAdmin = await permissionValidator.ValidateTeacherPermissions(
            userGuid,
            TeacherPermissions.AdminAccess
        );

        var archiveStudentPayload = new ArchiveStudentPayload
        {
            StudentGuid = request.StudentGuid,
            TeacherGuid = userGuid,
            IsAdmin = isAdmin.IsOk,
        };

        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(_ => Response.Ok(Unit.Default), Response.Error);
    }

    private static async Task<IResult> SetHealthGroup(
        [FromBody] SetHealthGroupRequest request,
        [FromServices] AddHealthGroupCommand command,
        HttpContext ctx
    )
    {
        var userGuid = ctx.User.Claims.First(c => c.Type == "IndividualGuid").Value;

        var res = await command.ExecuteAsync(
            new AddHealthGroupPayload
            {
                HealthGroup = request.HealthGroup,
                StudentGuid = request.StudentGuid,
                TeacherGuid = userGuid,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }

    private static async Task<IResult> SetSpecialization(
        [FromBody] SetSpecializationRequest request,
        [FromServices] SetSpecializationCommand command,
        HttpContext ctx
    )
    {
        var res = await command.ExecuteAsync(
            new SetSpecializationPayload
            {
                StudentGuid = request.StudentGuid,
                Specialization = request.Specialization,
            }
        );

        return res.Match(Response.Ok, Response.Error);
    }
}
