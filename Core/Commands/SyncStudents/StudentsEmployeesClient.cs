using System.Net.Http.Json;
using Serilog;

namespace Core.Commands.SyncStudents;

file sealed class StudentEducation
{
    public required string Group { get; set; }
    public required string Department { get; set; }
    public required int Course { get; set; }
    public required int StartYear { get; set; }
    public required string DegreeLevel { get; set; }
    public required bool IsStudying { get; set; }
}

file sealed class ResponseStudent
{
    public required string Id { get; set; }
    public required string FullName { get; set; }

    public required List<StudentEducation> Educations { get; init; }
}

file sealed class StudentsRes
{
    public required List<ResponseStudent> Data { get; init; }
}

public sealed class Employee
{
    public required string Id { get; init; }
    public required string FullName { get; init; }
}

file sealed class EmployeesRes
{
    public required List<Employee> Data { get; init; }
}

public sealed class Student
{
    public required string Guid { get; set; }
    public required string FullName { get; set; }
    public required string Group { get; set; }
    public required string Department { get; set; }
    public required int Course { get; set; }
}

public sealed class StudentsEmployeesClient
{
    private readonly HttpClient _httpClient;

    public StudentsEmployeesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("https://api.mospolytech.ru");
    }

    public async Task<List<Student>> GetStudentsAsync(int limit, int offset)
    {
        var response = await _httpClient.PostAsync(
            "lk/students/all",
            JsonContent.Create(new { limit, offset })
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = new Exception(await response.Content.ReadAsStringAsync());

            Log.Error(error, "Unknown error in Students sync");

            throw error;
        }

        var res = await response.Content.ReadFromJsonAsync<StudentsRes>();

        var students = res!
            .Data.Where(s => s.Educations.Any(e => e.IsStudying && e.Group != string.Empty))
            .Select(s =>
            {
                var education = s
                    .Educations.Where(e => e.IsStudying && e.Group != string.Empty)
                    .OrderBy(e => e.DegreeLevel == "Очная" ? 1 : 0)
                    .ThenByDescending(e => e.StartYear)
                    .First();

                return new Student
                {
                    Guid = s.Id,
                    FullName = s.FullName,
                    Group = education.Group,
                    Course = education.Course,
                    Department = education.Department,
                };
            })
            .ToList();

        return students;
    }

    public async Task<List<Employee>> GetEmployeesAsync(
        int limit,
        int offset,
        string fullNameFilter
    )
    {
        var response = await _httpClient.PostAsync(
            "lk/employees/all",
            JsonContent.Create(
                new
                {
                    limit,
                    offset,
                    fullName = fullNameFilter,
                }
            )
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = new Exception(await response.Content.ReadAsStringAsync());

            Log.Error(error, "Unknown error in Employees search");

            throw error;
        }

        var res = await response.Content.ReadFromJsonAsync<EmployeesRes>();

        return res!.Data;
    }
}
