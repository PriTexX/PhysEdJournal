using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace Test;

public class SemesterServiceTests
{
    private readonly ApplicationContext _context;
    private readonly SemesterService _semesterService;

    public SemesterServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase("Guid.NewGuid().ToString()")
            .Options;
        
        _context = new ApplicationContext(options);

        _semesterService = new SemesterService(_context);
    }
    
    [Fact]
    public async Task StartNewSemesterAsync_ValidName_ShouldCreateNewSemester()
    {
        // Arrange
        var validSemesterName = "2022-2023/spring";

        // Act
        var result = await _semesterService.StartNewSemesterAsync(validSemesterName);

        // Assert
        Assert.True(result.IsSuccess);
        
        var semester = await _context.Semesters.FirstOrDefaultAsync(s => s.Name == validSemesterName);
        Assert.NotNull(semester);
        
        Assert.Equal(semester.Name, validSemesterName);
        
        ClearDatabase();
    }
    
    [Fact]
    public async Task StartNewSemesterAsync_InvalidName_ShouldThrowException()
    {
        // Arrange
        var semesterName = "invalid_name";
    
        // Act
        var result = await _semesterService.StartNewSemesterAsync(semesterName);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<SemesterNameValidationException>(exception);
            return true;
        });

        ClearDatabase();
    }
    
    private void ClearDatabase()
    {
        _context.Semesters.RemoveRange(_context.Semesters);
        _context.SaveChanges();
    }
}