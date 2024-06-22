using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;

namespace Api.GraphQL;

public class Query
{
    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity?> GetStudent(ApplicationContext context, string guid)
    {
        return context.Students.Where(s => s.StudentGuid == guid);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StudentEntity> GetStudents(ApplicationContext context)
    {
        return context.Students;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ArchivedStudentEntity> GetArchivedStudents(ApplicationContext context)
    {
        return context.ArchivedStudents;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PointsStudentHistoryEntity?> GetPointsHistory(ApplicationContext context)
    {
        return context.PointsStudentsHistory;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<GroupEntity> GetGroups(ApplicationContext context)
    {
        return context.Groups;
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<TeacherEntity?> GetTeacher(ApplicationContext context, string guid)
    {
        return context.Teachers.Where(t => t.TeacherGuid == guid);
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TeacherEntity> GetTeachers(ApplicationContext context)
    {
        return context.Teachers;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SemesterEntity> GetSemesters(ApplicationContext context)
    {
        return context.Semesters;
    }

    public async Task<string> GetCurrentSemesterAsync(
        ApplicationContext context,
        [Service] ILogger<Query> logger
    )
    {
        try
        {
            var currentSemester = await context.Semesters.SingleAsync(s => s.IsCurrent == true);
            return currentSemester.Name;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error happened during querying current semester");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetCompetitions(
        ApplicationContext context,
        [Service] ILogger<Query> logger
    )
    {
        try
        {
            return await context
                .Competitions.Select(c => c.CompetitionName)
                .OrderBy(name => name)
                .ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error happened during querying competitions");
            throw;
        }
    }
}
