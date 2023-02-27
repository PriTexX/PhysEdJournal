using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.GraphQL;

public class Query
{
    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity?> GetStudent(ApplicationContext context, string guid)
    {
        return context.Students.Where(s => s.StudentGuid == guid);
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity> GetStudents(ApplicationContext context)
    {
        return context.Students;
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<ArchivedStudentEntity> GetArchivedStudents(ApplicationContext context)
    {
        return context.ArchivedStudents;
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<PointsStudentHistoryEntity?> GetStudentPointsHistory(ApplicationContext context, string guid)
    {
        return context.StudentsPointsHistory.Where(s => s.StudentGuid == guid);
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<PointsStudentHistoryEntity?> GetPointsHistory(ApplicationContext context)
    {
        return context.StudentsPointsHistory;
    }

    [UseProjection]
    [UseFiltering]
    public IQueryable<GroupEntity> GetGroups(ApplicationContext context)
    {
        return context.Groups;
    }
}