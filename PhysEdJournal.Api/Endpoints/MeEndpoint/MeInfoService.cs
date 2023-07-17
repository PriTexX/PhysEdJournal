using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.Endpoints.MeEndpoint;

public sealed class MeInfoService
{
    private readonly ApplicationContext _applicationContext;

    public MeInfoService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<LanguageExt.Common.Result<StudentInfoResponse>> GetStudentInfo(string studentGuid)
    {
        var studentActivity = await _applicationContext.Students
            .Where(s => s.StudentGuid == studentGuid)
            .Select(s => new {s.AdditionalPoints, s.PointsForStandards, s.Visits, s.Group.VisitValue})
            .FirstOrDefaultAsync();
        
        if (studentActivity is null)
        {
            return new LanguageExt.Common.Result<StudentInfoResponse>(new StudentNotFoundException(studentGuid));
        }
        
        var studentPoints = studentActivity.AdditionalPoints + studentActivity.PointsForStandards +
                            (studentActivity.Visits * studentActivity.VisitValue);

        return new StudentInfoResponse
        {
            Points = studentPoints
        };
    }

    public async Task<LanguageExt.Common.Result<ProfessorInfoResponse>> GetProfessorInfo(string professorGuid)
    {
        var teacherPermissions = await _applicationContext.Teachers
            .Where(t => t.TeacherGuid == professorGuid)
            .Select(t => new { t.Permissions })
            .FirstOrDefaultAsync();

        if (teacherPermissions is null)
        {
            return new LanguageExt.Common.Result<ProfessorInfoResponse>(new TeacherNotFoundException(professorGuid));
        }

        var textTeacherPermissions = teacherPermissions.Permissions
            .ToString()
            .Split(",")
            .Select(s => s.Trim())
            .ToList();

        return new ProfessorInfoResponse
        {
            Permisions = textTeacherPermissions 
        };
    }
}