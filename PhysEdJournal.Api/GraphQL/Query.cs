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
    
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    public IQueryable<StudentEntity> GetStudents(ApplicationContext context)
    {
        return context.Students;
    }
    
    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    public IQueryable<ArchivedStudentEntity> GetArchivedStudents(ApplicationContext context)
    {
        return context.ArchivedStudents;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
    public IQueryable<PointsStudentHistoryEntity?> GetPointsHistory(ApplicationContext context)
    {
        return context.StudentsPointsHistory;
    }

    [UseOffsetPaging]
    [UseProjection]
    [UseFiltering]
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
    public IQueryable<TeacherEntity> GetTeachers(ApplicationContext context)
    {
        return context.Teachers;
    }
}