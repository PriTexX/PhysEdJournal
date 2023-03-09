using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace Test;

public class StudentServiceTests
{
    private readonly ApplicationContext _context;
    private readonly StudentService _studentService;

    public StudentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "Test Db")
            .Options;
        
        _context = new ApplicationContext(options);

        var serviceOptions = Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = "null",
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0
        });
        var groupService = new GroupService(_context, serviceOptions);
        
        _studentService = new StudentService(_context, serviceOptions, groupService);
    }

    [Fact]
    public async Task AddPointsAsync_StudentNotFound_ShouldThrowException()
    {
        var historyEntity = DefaultPointsStudentHistoryEntity();

        var result = await _studentService.AddPointsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFound>(exception);
            return true;
        });
        
        ClearDatabase();
    }

    [Fact]
    private async Task AddPointsAsync_AddsPointsToStudentAndHistory_ShouldWorkProperly()
    {
        var student = DefaultStudentEntity();
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        var historyEntity = DefaultPointsStudentHistoryEntity(studentGuid: student.StudentGuid);

        var result = await _studentService.AddPointsAsync(historyEntity);
        
        Assert.True(result.IsSuccess);
        var dbStudent = _context.Students.FirstOrDefault(s => s.StudentGuid == student.StudentGuid);
        Assert.NotNull(dbStudent);
        
        ClearDatabase();
    }

    private void ClearDatabase()
    {
        _context.Students.RemoveRange(_context.Students);
        _context.StudentsPointsHistory.RemoveRange(_context.StudentsPointsHistory);
        _context.StudentsVisitsHistory.RemoveRange(_context.StudentsVisitsHistory);
        _context.ArchivedStudents.RemoveRange(_context.ArchivedStudents);
        _context.SaveChanges();
    }

    private StudentEntity DefaultStudentEntity(string name = "DefaultName")
    {
        var student = new StudentEntity
        {
            StudentGuid = Guid.NewGuid().ToString(),
            FullName = "John Smith",
            GroupNumber = "Group1",
            HasDebtFromPreviousSemester = true,
            ArchivedVisitValue = 10.5,
            AdditionalPoints = 2,
            Visits = 10,
            Course = 2,
            HealthGroup = 1,
            Department = "IT",
        };

        return student;
    }

    private PointsStudentHistoryEntity DefaultPointsStudentHistoryEntity(string name = "DefaultName", string studentGuid = "DefaultGuid")
    {
        var historyEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            Date = DateOnly.MaxValue,
            TeacherGuid = Guid.NewGuid().ToString(),
            WorkType = WorkType.Standards,
            Points = 10
        };

        return historyEntity;
    }
}