using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.GraphQL;

public class Query
{
    public StudentEntity? GetStudent([Service] ApplicationContext context)
    {
        return context.Students.FirstOrDefault();
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
    public IQueryable<GroupEntity> GetGroups([Service] ApplicationContext context)
    {
        return context.Groups;
    }
}