using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests;

public class TeacherServiceTests
{
    private readonly DbContextOptions<ApplicationContext> _contextOptions;

    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions);
    }

    private TeacherService CreateTeacherService(ApplicationContext context)
    {
        var serviceOptions = Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = "null",
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0
        });

        var teacherService = new TeacherService(context);

        return teacherService;
    }

    public TeacherServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Theory]
    [InlineData(TeacherPermissions.AdminAccess)]
    [InlineData(TeacherPermissions.DefaultAccess)]
    [InlineData(TeacherPermissions.SecretaryAccess)]
    [InlineData(TeacherPermissions.OnlineCourseAccess)]
    public async Task GivePermissionsAsync_WhenValidInput_ShouldChangeTeacherPermissions(TeacherPermissions permissions)
    {
        // Arrange
        var context = CreateContext();
        var teacherService = CreateTeacherService(context);
        var expectedPermissions = permissions;
        var teacher = DefaultTeacherEntity(permissions);
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await teacherService.GivePermissionsAsync(teacher.TeacherGuid, permissions);

        // Assert
        Assert.True(result.IsSuccess);

        var teacherFromDb = await context.Teachers.FindAsync(teacher.TeacherGuid);
        Assert.Equal(expectedPermissions, teacherFromDb?.Permissions);
    }
    
    [Fact]
    public async Task GivePermissionsAsync_WhenTeacherNotExists_ShouldReturnTeacherNotFound()
    {
        // Arrange
        var context = CreateContext();
        var teacherService = CreateTeacherService(context);
        var teacher = DefaultTeacherEntity();

        // Act
        var result = await teacherService.GivePermissionsAsync(teacher.TeacherGuid, TeacherPermissions.AdminAccess);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<TeacherNotFoundException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task? CreateTeacherAsync_WhenValidInput_ShouldAddTeacherToDb()
    {
        // Arrange
        var context = CreateContext();
        var teacherService = CreateTeacherService(context);
        var teacher = DefaultTeacherEntity();
        
        // Act
        var result = await teacherService.CreateTeacherAsync(teacher);

        // Assert
        Assert.True(result.IsSuccess);

        var teacherFromDb = await context.Teachers.FindAsync(teacher.TeacherGuid);
        Assert.NotNull(teacherFromDb);
        Assert.Equal(teacher.TeacherGuid, teacherFromDb?.TeacherGuid);
    }
    
    [Fact]
    public async Task CreateTeacherAsync_WhenTeacherAlreadyExists_ShouldThrowTeacherAlreadyExistsException()
    {
        // Arrange
        var context = CreateContext();
        var teacherService = CreateTeacherService(context);
        var teacher = DefaultTeacherEntity();
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();
        
        // Act
        var result = await teacherService.CreateTeacherAsync(teacher);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<TeacherAlreadyExistsException>(exception);
            return true;
        });
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
}