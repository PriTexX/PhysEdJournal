using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests;

public class SemesterServiceTests
{
    /*private readonly DbContextOptions<ApplicationContext> _contextOptions;

    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions);
    }

    private SemesterService CreateSemesterService(ApplicationContext context)
    {
        return new SemesterService(context);
    }

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
        var validSemesterName = "2022-2023/spring";

        // Act
        var result = await semesterService.StartNewSemesterAsync(validSemesterName);

        // Assert
        Assert.True(result.IsSuccess);
        
        var semester = await context.Semesters.FirstOrDefaultAsync(s => s.Name == validSemesterName);
        Assert.NotNull(semester);
        
        Assert.Equal(semester.Name, validSemesterName);
    }
    
    [Fact]
    public async Task StartNewSemesterAsync_InvalidName_ShouldThrowException()
    {
        // Arrange
        var context = CreateContext();
        var semesterService = CreateSemesterService(context);
        var semesterName = "invalid_name";
    
        // Act
        var result = await semesterService.StartNewSemesterAsync(semesterName);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<SemesterNameValidationException>(exception);
            return true;
        });
    }*/
}