using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Models;

namespace PhysEdJournal.Infrastructure.Services.StaticFunctions;

public static class StudentServiceFunctions
{
    public static (IEnumerable<TSource>, IEnumerable<TSource>) Partition<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        var matches = new List<TSource>();
        var nonMatches = new List<TSource>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                matches.Add(item);
            }
            else
            {
                nonMatches.Add(item);
            }
        }

        return (matches, nonMatches);
    }
    
    public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<TResult>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            var result = await selector(item);
            results.Add(result);
        }
        return results;
    }
    
    public static async IAsyncEnumerable<Student> GetAllStudentsAsync(string url, int pageSize)
    {
        var query = @"query($pageSize: Int!, $skipSize: Int!) {
            students(take: $pageSize, skip: $skipSize, filter: ""!string.IsNullOrEmpty(group)"") {
                hasNextPage
                items {
                    guid
                    fullName
                    group
                    course
                    department
                }
            }
         }";
       
        var client = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
        var skipSize = 0;

        while (true)
        {
            var request = new GraphQLRequest { Query = query, Variables = new { pageSize, skipSize }};
            var response = await client.SendQueryAsync<PagedGraphQLStudent>(request);

            if (response.Errors != null && response.Errors.Any())
            {
                var errors = response.Errors.Select(e => new Exception(e.Message)).ToList();
                throw new AggregateException(errors);
            }

            foreach (var student in response.Data.Students.Items)
            {
                yield return student;
            }
            
            if(!response.Data.Students.HasNextPage)
                break;

            skipSize += pageSize;
        }
    }
    
    public static async Task<IEnumerable<StudentEntity>> GetManyStudentsWithManyKeys(ApplicationContext applicationContext, string[] keys)
    {
        return await applicationContext.Students.Where(s => keys.Contains(s.StudentGuid)).ToListAsync();
    }

    public static async Task CommitChangesToContext(ApplicationContext applicationContext, List<(bool, StudentEntity)> students)
    {
        applicationContext.Students.AddRange(students
            .Where(d => d.Item1)
            .Where(p => p.Item2.GroupNumber != "")
            .Select(p => p.Item2));
        
        applicationContext.Students.UpdateRange(students
            .Where(d => !d.Item1)
            .Where(p => p.Item2.GroupNumber != "")
            .Select(p => p.Item2));

        await applicationContext.SaveChangesAsync();
    }

    public static (bool, StudentEntity) GetUpdatedOrCreatedStudentEntities(Student studentModel, StudentEntity? dbStudent)
    {
        if (dbStudent is null)
        {
            return (true, CreateStudentEntityFromStudentModel(studentModel)); // if it is newly created entity then return true otherwise false
        }

        if (dbStudent.GroupNumber != studentModel.Group)
            dbStudent.GroupNumber = studentModel.Group;

        if (dbStudent.FullName != studentModel.FullName)
            dbStudent.FullName = studentModel.FullName;
        
        if (dbStudent.Course != studentModel.Course)
            dbStudent.Course = studentModel.Course;
        
        if (dbStudent.Department != studentModel.Department)
            dbStudent.Department = studentModel.Department;

        return (false, dbStudent);
    }

    private static StudentEntity CreateStudentEntityFromStudentModel(Student student)
    {
        return new StudentEntity
        {
            StudentGuid = student.Guid,
            FullName = student.FullName,
            GroupNumber = student.Group,
            Course = student.Course,
            Department = student.Department
        };
    }
}