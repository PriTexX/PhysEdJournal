using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class StudentMutationExtensions
{
    
    [Error(typeof(StudentNotFound))]
    public async Task<Success> AddPointsToStudent([Service] IStudentService studentService, 
        string studentGuid, string teacherGuid, 
        int pointsAmount, DateOnly date, 
        WorkType workType, string currentSemesterName, 
        string? comment = null)
    {
        var pointsHistory = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            TeacherGuid = teacherGuid,
            Points = pointsAmount,
            Date = date,
            WorkType = workType,
            SemesterName = currentSemesterName,
            Comment = comment
        };
        var res = await studentService.AddPointsAsync(pointsHistory);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(StudentNotFound))]
    public async Task<Success> IncreaseStudentVisits(string studentGuid, DateOnly date, string teacherGuid, [Service] IStudentService studentService)
    {
        var res = await studentService.IncreaseVisitsAsync(studentGuid, date, teacherGuid);

        return res.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(StudentNotFound))]
    public async Task<ArchivedStudentEntity> ArchiveStudent([Service] IStudentService studentService, string studentGuid, string currentSemesterName, bool isForceMode = false)
    {
        var res = await studentService.ArchiveStudentAsync(studentGuid, currentSemesterName, isForceMode);

        return res.Match(archivedStudent => archivedStudent, exception => throw exception);
    }

    public async Task<Success> UpdateStudentsInfo([Service] IStudentService studentService)
    {
        var res = await studentService.UpdateStudentsInfoAsync();

        return res.Match(_ => true, exception => throw exception);
    }
}