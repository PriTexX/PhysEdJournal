using System.Net.Mime;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class StudentService
{
    private readonly ApplicationContext _applicationContext;
    
    public StudentService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    private void Create(StudentEntity studentEntity)
    {
        _applicationContext.Students.Add(studentEntity);
    }

    private async Task<StudentEntity?> Read(StudentEntity studentEntity)
    {
        var student = await _applicationContext.Students.FindAsync(studentEntity);

        return student;
    }

    private void Update(StudentEntity studentEntity)
    {
        _applicationContext.Students.Update(studentEntity);
    }

    private void Delete(StudentEntity studentEntity)
    {
        _applicationContext.Students.Remove(studentEntity);
    }
}