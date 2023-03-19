using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests;

public class StudentServiceTests
{
    /*private readonly DbContextOptions<ApplicationContext> _contextOptions;

    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions);
    }

    private StudentService CreateStudentService(ApplicationContext context)
    {
        var serviceOptions = Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = "null",
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0
        });
        var groupService = new GroupService(context, serviceOptions);
        
        var studentService = new StudentService(context, serviceOptions, groupService);

        return studentService;
    }

    public StudentServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddPointsAsync_StudentNotFound_ShouldThrowException()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var historyEntity = DefaultPointsStudentHistoryEntity();

        var result = await studentService.AddPointsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFound>(exception);
            return true;
        });
    }

    [Fact]
    private async Task AddPointsAsync_AddsPointsToStudentAndHistory_ShouldWorkProperly()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity(); 
        context.Students.Add(student);
        await context.SaveChangesAsync();
        var historyEntity = DefaultPointsStudentHistoryEntity(studentGuid: student.StudentGuid);

        var result = await studentService.AddPointsAsync(historyEntity);
        
        Assert.True(result.IsSuccess);
        var dbStudent = context.Students.FirstOrDefault(s => s.StudentGuid == student.StudentGuid);
        Assert.NotNull(dbStudent);
    }
    
    [Fact]
    public async Task IncreaseVisitsAsync_WhenValidInput_ShouldIncrementVisits()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var expectedVisits = ++DefaultStudentEntity().Visits;
        var date = new DateOnly(2023, 3, 9);
        var teacherGuid = Guid.NewGuid().ToString();
        var student = DefaultStudentEntity();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacherGuid);

        // Assert
        Assert.True(result.IsSuccess);
        
        var updatedStudent = await context.Students.FindAsync(student.StudentGuid);
        Assert.Equal(expectedVisits, updatedStudent.Visits);
    }

    [Fact]
    public async Task IncreaseVisitsAsync_WhenInvalidStudentGuid_ShouldIncrementVisits()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = new DateOnly(2023, 3, 9);
        var teacherGuid = Guid.NewGuid().ToString();
        var student = DefaultStudentEntity();

        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacherGuid);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFound>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task IncreaseVisitsAsync_WhenValidInput_ShouldAddVisitToHistory()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = new DateOnly(2023, 3, 9);
        var teacherGuid = Guid.NewGuid().ToString();
        var student = DefaultStudentEntity();
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
    
        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacherGuid);
    
        // Assert
        Assert.True(result.IsSuccess);
        
        var record = await context.StudentsVisitsHistory.FirstOrDefaultAsync(h => h.StudentGuid == student.StudentGuid);
        Assert.NotNull(record);
        Assert.Equal(date, record.Date);
        Assert.Equal(student.StudentGuid, record.StudentGuid);
        Assert.Equal(teacherGuid, record.TeacherGuid);
    }

    private StudentEntity DefaultStudentEntity(bool hasDebt = false)
    {
        var student = new StudentEntity
        {
            StudentGuid = Guid.NewGuid().ToString(),
            FullName = "John Smith",
            GroupNumber = "Group1",
            HasDebtFromPreviousSemester = hasDebt,
            ArchivedVisitValue = 10.5,
            AdditionalPoints = 2,
            Visits = 10,
            Course = 2,
            HealthGroup = 1,
            Department = "IT",
        };

        return student;
    }
    
    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
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
    }*/
}