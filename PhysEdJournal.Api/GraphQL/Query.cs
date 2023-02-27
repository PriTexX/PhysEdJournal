using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.GraphQL;

public class Query
{
    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity?> GetStudent([Service] ApplicationContext context, string guid)
    {
        return context.Students.Where(s => s.StudentGuid == guid);
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity> GetStudents([Service] ApplicationContext context)
    {
        return context.Students;
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<ArchivedStudentEntity> GetArchivedStudents([Service] ApplicationContext context)
    {
        return context.ArchivedStudents;
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<PointsStudentHistoryEntity?> GetStudentPointsHistory([Service] ApplicationContext context, string guid)
    {
        return context.StudentsPointsHistory.Where(s => s.StudentGuid == guid);
    }
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<PointsStudentHistoryEntity?> GetPointsHistory([Service] ApplicationContext context)
    {
        return context.StudentsPointsHistory;
    }

    [UseProjection]
    [UseFiltering]
    public IQueryable<GroupEntity> GetGroups([Service] ApplicationContext context)
    {
        return context.Groups;
    }
}