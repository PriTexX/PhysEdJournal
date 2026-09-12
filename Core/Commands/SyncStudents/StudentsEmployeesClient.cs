using System.Net.Http.Json;
using Serilog;

namespace Core.Commands.SyncStudents;

public sealed class StudentEducation
{
    public required string Group { get; set; }
    public required string Department { get; set; }
    public required int Course { get; set; }
    public required int StartYear { get; set; }
    public required string DegreeLevel { get; set; }
    public required bool IsStudying { get; set; }
}

public sealed class ResponseStudent
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

public sealed class StudentsEmployeesClient
{
    private readonly HttpClient _httpClient;

    public StudentsEmployeesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("https://api.mospolytech.ru");
    }

    public async Task<List<ResponseStudent>> GetStudentsAsync(int limit, int offset)
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

        return res!.Data;
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
