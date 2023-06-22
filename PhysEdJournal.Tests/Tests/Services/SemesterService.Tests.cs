using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests.Tests.Services;

public class SemesterServiceTests
{
    private readonly DbContextOptions<ApplicationContext> _contextOptions;

    public SemesterServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task StartNewSemesterAsync_ValidName_ShouldCreateNewSemester()
    {
        // Arrange
        var context = CreateContext();
        var semesterService = CreateSemesterService(context);
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var validSemesterName = "2022-2023/spring";
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();

        // Act
        var result = await semesterService.StartNewSemesterAsync(validSemesterName);

        // Assert
        Assert.True(result.IsSuccess);
        
        var semester = await context.Semesters.FirstOrDefaultAsync(s => s.Name == validSemesterName);
        Assert.NotNull(semester);
        
        Assert.Equal(semester.Name, validSemesterName);
    }

    [Fact]
    public async Task StartNewSemesterAsync_InvalidName_ShouldReturnSemesterNameValidationException()
    {
        // Arrange
        var context = CreateContext();
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var semesterService = CreateSemesterService(context);
        var semesterName = "invalid_name";
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();
    
        // Act
        var result = await semesterService.StartNewSemesterAsync(semesterName);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<SemesterNameValidationException>(exception);
            return true;
        });
    }
    
    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions, CreateMemoryCache());
    }
    
    private MemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    private SemesterService CreateSemesterService(ApplicationContext context)
    {
        return new SemesterService(context, CreateMemoryCache());
    }
}