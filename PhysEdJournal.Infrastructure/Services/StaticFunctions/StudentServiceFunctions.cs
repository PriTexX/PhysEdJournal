using System.Globalization;
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
    public static async IAsyncEnumerable<Student> GetAllStudentsAsync(string url, int pageSize)
    {
        var query =
            @"query($pageSize: Int!, $skipSize: Int!) {
            students(take: $pageSize, skip: $skipSize, where: {group: {neq: """"}}) {
                pageInfo{hasNextPage}
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
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new { pageSize, skipSize },
            };
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

            if (!response.Data.Students.PageInfo.HasNextPage)
            {
                break;
            }

            skipSize += pageSize;
        }
    }

    public static async Task<IEnumerable<StudentEntity>> GetManyStudentsWithManyKeys(
        ApplicationContext applicationContext,
        string[] keys
    )
    {
        return await applicationContext.Students
            .Where(s => keys.Contains(s.StudentGuid))
            .ToListAsync();
    }

    public static async Task CommitChangesToContext(
        ApplicationContext applicationContext,
        List<(bool, StudentEntity)> students
    )
    {
        applicationContext.Students.AddRange(
            students
                .Where(d => d.Item1)
                .Where(p => p.Item2.GroupNumber != string.Empty)
                .Where(p => p.Item2.GroupNumber[2] == '1')
                .Select(p =>
                {
                    p.Item2.FullName = PrettifyFullName(p.Item2.FullName);
                    return p;
                })
                .Select(p => p.Item2)
        );

        applicationContext.Students.UpdateRange(
            students
                .Where(d => !d.Item1)
                .Where(p => p.Item2.GroupNumber != string.Empty)
                .Select(p => p.Item2)
        );

        await applicationContext.SaveChangesAsync();
    }

    private static string PrettifyFullName(string fullName)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fullName.ToLower().Trim());
    }

    public static (bool, StudentEntity) GetUpdatedOrCreatedStudentEntities(
        Student studentModel,
        StudentEntity? dbStudent,
        string currentSemesterName
    )
    {
        if (dbStudent is null)
        {
            return (true, CreateStudentEntityFromStudentModel(studentModel, currentSemesterName)); // if it is newly created entity then return true otherwise false
        }

        if (dbStudent.GroupNumber != studentModel.Group)
        {
            dbStudent.GroupNumber = studentModel.Group;
        }

        if (dbStudent.FullName != studentModel.FullName)
        {
            dbStudent.FullName = studentModel.FullName;
        }

        if (dbStudent.Course != studentModel.Course)
        {
            dbStudent.Course = studentModel.Course;
        }

        if (dbStudent.Department != studentModel.Department)
        {
            dbStudent.Department = studentModel.Department;
        }

        return (false, dbStudent);
    }

    private static StudentEntity CreateStudentEntityFromStudentModel(
        Student student,
        string currentSemesterName
    )
    {
        return new StudentEntity
        {
            StudentGuid = student.Guid,
            FullName = student.FullName,
            GroupNumber = student.Group,
            Course = student.Course,
            Department = student.Department,
            CurrentSemesterName = currentSemesterName,
        };
    }
}
