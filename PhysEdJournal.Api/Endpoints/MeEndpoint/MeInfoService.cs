using Core.Commands;
using DB;
using Microsoft.EntityFrameworkCore;

namespace PhysEdJournal.Api.Endpoints.MeEndpoint;

public sealed class MeInfoService
{
    private readonly ApplicationContext _applicationContext;

    public MeInfoService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<PResult.Result<StudentInfoResponse>> GetStudentInfo(string studentGuid)
    {
        var studentActivity = await _applicationContext
            .Students.Where(s => s.StudentGuid == studentGuid)
            .Select(s => new
            {
                s.AdditionalPoints,
                s.PointsForStandards,
                s.Visits,
                s.Group!.VisitValue,
            })
            .FirstOrDefaultAsync();

        if (studentActivity is null)
        {
            return new StudentNotFoundError();
        }

        var studentPoints =
            studentActivity.AdditionalPoints
            + studentActivity.PointsForStandards
            + (studentActivity.Visits * studentActivity.VisitValue);

        return new StudentInfoResponse { Points = studentPoints };
    }

    public async Task<PResult.Result<ProfessorInfoResponse>> GetProfessorInfo(string professorGuid)
    {
        var teacherPermissions = await _applicationContext
            .Teachers.Where(t => t.TeacherGuid == professorGuid)
            .Select(t => new { t.Permissions })
            .FirstOrDefaultAsync();

        if (teacherPermissions is null)
        {
            return new TeacherNotFoundError();
        }

        var textTeacherPermissions = teacherPermissions
            .Permissions.ToString()
            .Split(",")
            .Select(s => s.Trim())
            .ToList();

        return new ProfessorInfoResponse { Permisions = textTeacherPermissions };
    }
}
