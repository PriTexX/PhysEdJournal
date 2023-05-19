using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using PhysEdJournal.Infrastructure.Validators.Standards;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Tests;

public class StudentServiceTests
{
    private readonly DbContextOptions<ApplicationContext> _contextOptions;

    public StudentServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }
    
    [Theory]
    [InlineData(WorkType.OnlineWork)]
    [InlineData(WorkType.ExternalFitness)]
    [InlineData(WorkType.Activist)]
    [InlineData(WorkType.Science)]
    [InlineData(WorkType.GTO)]
    [InlineData(WorkType.InternalTeam)]
    [InlineData(WorkType.Competition)]
    private async Task AddPointsAsync_AddsPointsToStudent_ShouldWorkProperly(WorkType workType)
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student); 
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultPointsStudentHistoryEntity(studentGuid: student.StudentGuid, workType: workType, teacherGuid: teacher.TeacherGuid);
    
        var result = await studentService.AddPointsAsync(historyEntity);
        
        Assert.True(result.IsSuccess);
        var studentFromDb = await context.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.PointsStudentHistory.Contains(historyEntity));
    }
    
    [Theory]
    [InlineData(WorkType.OnlineWork)]
    [InlineData(WorkType.ExternalFitness)]
    [InlineData(WorkType.Activist)]
    [InlineData(WorkType.Science)]
    [InlineData(WorkType.GTO)]
    [InlineData(WorkType.InternalTeam)]
    [InlineData(WorkType.Competition)]
    private async Task AddPointsAsync_ActionFromFutureException_ShouldThrowException(WorkType workType)
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultPointsStudentHistoryEntity(studentGuid: student.StudentGuid, workType: workType, teacherGuid: teacher.TeacherGuid, date: DateOnly.MaxValue);
    
        var result = await studentService.AddPointsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<ActionFromFutureException>(exception);
            return true;
        });
    }

    [Fact]
    public async Task AddPointsAsync_StudentNotFoundException_ShouldThrowException()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var historyEntity = DefaultPointsStudentHistoryEntity();

        var result = await studentService.AddPointsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFoundException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task IncreaseVisitsAsync_IncreasesStudentsTotalVisits_ShouldWorkProperly()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = DateOnly.FromDateTime(DateTime.Today);
        var teacher = DefaultTeacherEntity(TeacherPermissions.SuperUser);
        var student = DefaultStudentEntity();

        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
    
        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
    
        // Assert
        Assert.True(result.IsSuccess);
        var studentFromDb = await context.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.Equal(studentFromDb.Visits, student.Visits);
    }
    
    [Fact]
    public async Task IncreaseVisitsAsync_StudentNotFoundException_ShouldThrowException()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = DateOnly.FromDateTime(DateTime.Today);
        var teacher = DefaultTeacherEntity(TeacherPermissions.SuperUser);
        var student = DefaultStudentEntity();

        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
    
        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFoundException>(exception);
            return true;
        });
    }

    [Fact]
    public async Task IncreaseVisitsAsync_ActionFromFutureException_ShouldThrowException()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = DateOnly.MaxValue;
        var teacher = DefaultTeacherEntity(TeacherPermissions.SuperUser);
        var student = DefaultStudentEntity();

        await context.Teachers.AddAsync(teacher);
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
    
        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<ActionFromFutureException>(exception);
            return true;
        });
    }

    [Fact]
    public async Task IncreaseVisitsAsync_VisitExpiredException_ShouldThrowException()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var date = new DateOnly(today.Year, today.Month, today.Day - (VISIT_LIFE_DAYS + 1));
        var teacher = DefaultTeacherEntity(TeacherPermissions.SuperUser);
        var student = DefaultStudentEntity();

        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
    
        // Act
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<VisitExpiredException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task IncreaseVisitsAsync_VisitAlreadyExistsException_ShouldThrowException()
    {
        // Arrange
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var date = DateOnly.FromDateTime(DateTime.Today);
        var teacher = DefaultTeacherEntity(TeacherPermissions.SuperUser);
        var student = DefaultStudentEntity();

        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
    
        // Act
        var prevRes = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
        Assert.True(prevRes.IsSuccess);
        
        var result = await studentService.IncreaseVisitsAsync(student.StudentGuid, date, teacher.TeacherGuid);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<VisitAlreadyExistsException>(exception);
            return true;
        });
    }
    
    [Theory]
    [InlineData(StandardType.Jumps)]
    [InlineData(StandardType.Squats)]
    [InlineData(StandardType.Tilts)]
    [InlineData(StandardType.PullUps)]
    [InlineData(StandardType.TorsoLifts)]
    [InlineData(StandardType.ShuttleRun)]
    [InlineData(StandardType.JumpingRopeJumps)]
    [InlineData(StandardType.FlexionAndExtensionOfArms)]
    public async Task AddPointsForStandardsAsync_AddsPointsForDifferentStandardsToStudent_ShouldWorkProperly(StandardType standardType)
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, standardType: standardType, teacherGuid: teacher.TeacherGuid);
    
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.True(result.IsSuccess);
        var studentFromDb = await context.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.StandardsStudentHistory.Contains(historyEntity));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(10)]
    public async Task AddPointsForStandardsAsync_AddsProperPointsForStandardToStudent_ShouldWorkProperly(int points)
    {
        var standardType = StandardType.Jumps;
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, standardType: standardType, teacherGuid: teacher.TeacherGuid, points: points);
    
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.True(result.IsSuccess);
        var studentFromDb = await context.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.StandardsStudentHistory.Contains(historyEntity));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    public async Task AddPointsForStandardsAsync_NonRegularPointsValueException_ShouldThrowException(int points)
    {
        var standardType = StandardType.Jumps;
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, standardType: standardType, teacherGuid: teacher.TeacherGuid, points: points);
    
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<NonRegularPointsValueException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task AddPointsForStandardsAsync_ActionFromFutureException_ShouldThrowException()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var date = DateOnly.MaxValue;
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, date: date, teacherGuid: teacher.TeacherGuid);
    
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<ActionFromFutureException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task AddPointsForStandardsAsync_StudentNotFoundException_ShouldThrowException()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, teacherGuid: teacher.TeacherGuid);
    
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFoundException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task AddPointsForStandardsAsync_StandardAlreadyExistsException_ShouldThrowException()
    {
        var context = CreateContext();
        var studentService = CreateStudentService(context);
        var student = DefaultStudentEntity();
        var teacher = DefaultTeacherEntity(permissions: TeacherPermissions.SuperUser);
        
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
        
        var historyEntity = DefaultStandardsHistoryEntity(studentGuid: student.StudentGuid, teacherGuid: teacher.TeacherGuid);
    
        var firstTry = await studentService.AddPointsForStandardsAsync(historyEntity);
        var result = await studentService.AddPointsForStandardsAsync(historyEntity);
        
        Assert.True(firstTry.IsSuccess);
        Assert.False(result.IsSuccess);
        firstTry.Match(_ => true, exception =>
        {
            Assert.IsType<StandardAlreadyExistsException>(exception);
            return true;
        });
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
            HealthGroup = HealthGroupType.Basic,
            Department = "IT",
            CurrentSemesterName = "2022-2023/spring"
        };

        return student;
    }
    
    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    private StandardsStudentHistoryEntity DefaultStandardsHistoryEntity(string studentGuid = "DefaultGuid", StandardType standardType = StandardType.Jumps, string teacherGuid = "DefaultGuid", DateOnly date = default, int points = 2)
    {
        var historyEntity = new StandardsStudentHistoryEntity()
        {
            StudentGuid = studentGuid,
            Date = date == default ? DateOnly.FromDateTime(DateTime.Today) : date,
            TeacherGuid = teacherGuid,
            StandardType = standardType,
            Points = points,
        };

        return historyEntity;
    }

    private PointsStudentHistoryEntity DefaultPointsStudentHistoryEntity( string studentGuid = "DefaultGuid", WorkType workType = WorkType.ExternalFitness, string teacherGuid = "DefaultGuid", DateOnly date = default)
    {
        var historyEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            Date = date == default ? DateOnly.FromDateTime(DateTime.Today) : date,
            TeacherGuid = teacherGuid,
            WorkType = workType,
            Points = 10
        };

        return historyEntity;
    }
    
    private TeacherEntity DefaultTeacherEntity(TeacherPermissions permissions = TeacherPermissions.DefaultAccess)
    {
        var teacher = new TeacherEntity()
        {
            FullName = "DefaultName",
            TeacherGuid = Guid.NewGuid().ToString(),
            Permissions = permissions
        };
        return teacher;
    }
    
    private MemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions, CreateMemoryCache());
    }

    private StudentService CreateStudentService(ApplicationContext context)
    {
        var serviceOptions = Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = "null",
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0,
            RsaPublicKey = null
        });
        var groupService = new GroupService(context, Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = null,
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0,
            RsaPublicKey = null
        }));
        
        var studentService = new StudentService(context, serviceOptions, groupService, new StandardsValidator());

        return studentService;
    }
}