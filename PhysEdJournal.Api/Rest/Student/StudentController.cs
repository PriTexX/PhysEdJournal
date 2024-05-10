using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Api.Rest.Common;
using PhysEdJournal.Api.Rest.Common.Filters;
using PhysEdJournal.Api.Rest.Student.Contracts;
using PhysEdJournal.Api.Rest.Student.Contracts.Responses;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.Rest.Student;

public static class StudentController
{
    public static void MapEndpoints(IEndpointRouteBuilder router)
    {
        ErrorHandler.AddErrors(StudentErrors.Errors);

        var studentRouter = router.MapGroup("/student");

        studentRouter
            .MapPost("/archive", ArchiveStudent)
            .AddValidation(ArchiveStudentRequest.GetValidator())
            .AddPermissionsValidation(TeacherPermissions.AdminAccess);

        studentRouter.MapGet("/{guid}", GetStudent);

        studentRouter.MapGet("/", GetStudents);
    }

    private static async Task<IResult> GetStudent(
        string guid,
        [FromServices] ApplicationContext context
    )
    {
        var student = await context
            .Students.Where(s => s.StudentGuid == guid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .FirstOrDefaultAsync();

        if (student is null)
        {
            return Response.Error(new StudentNotFoundException(guid));
        }

        var response = new GetStudentResponse
        {
            StudentGuid = student.StudentGuid,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            CurrentSemesterName = student.CurrentSemesterName,
            HasDebtFromPreviousSemester = student.HasDebtFromPreviousSemester,
            HadDebtInSemester = student.HadDebtInSemester,
            ArchivedVisitValue = student.ArchivedVisitValue,
            AdditionalPoints = student.AdditionalPoints,
            PointsForStandards = student.PointsForStandards,
            IsActive = student.IsActive,
            Visits = student.Visits,
            Course = student.Course,
            HealthGroup = student.HealthGroup,
            Department = student.Department,
            Version = student.Version,
            PointsStudentHistory = student.PointsStudentHistory?.ToList(),
            VisitsStudentHistory = student.VisitsStudentHistory?.ToList(),
            StandardsStudentHistory = student.StandardsStudentHistory?.ToList(),
        };

        return Response.Ok(response);
    }

    private static async Task<IResult> GetStudents(
        [AsParameters] StudentFilterParameters filter,
        [AsParameters] PaginationParameters paginationParameters,
        [FromQuery] string? sortingParams,
        [FromServices] ApplicationContext context
    )
    {
        IQueryable<StudentEntity> query = context.Students;

        query = query.Where(s => s.IsActive == filter.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            query = query.Where(s => EF.Functions.ILike(s.FullName, $"%{filter.FullName}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.GroupNumber))
        {
            query = query.Where(s => string.Equals(s.GroupNumber, filter.GroupNumber));
        }

        if (filter.Course is not null)
        {
            query = query.Where(s => Equals(s.Course, filter.Course));
        }

        query = sortingParams is not null
            ? OrderByAddons.ApplySort(query, sortingParams)
            : query.OrderBy(s => s.FullName);

        var data = await query
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        var response = data.Select(stud => new GetStudentsResponse
            {
                StudentGuid = stud.StudentGuid,
                FullName = stud.FullName,
                GroupNumber = stud.GroupNumber,
                IsActive = stud.IsActive,
            })
            .ToList();

        return Response.Ok(response);
    }

    private static async Task<IResult> ArchiveStudent(
        [FromBody] ArchiveStudentRequest request,
        [FromServices] ArchiveStudentCommand archiveStudentCommand,
        [FromServices] PermissionValidator permissionValidator
    )
    {
        var archiveStudentPayload = new ArchiveStudentCommandPayload
        {
            StudentGuid = request.StudentGuid,
            SemesterName = request.SemesterName,
        };

        var res = await archiveStudentCommand.ExecuteAsync(archiveStudentPayload);

        return res.Match(Response.Ok, Response.Error);
    }
}
