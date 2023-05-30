using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class StartNewSemesterCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task StartNewSemesterAsync_ValidName_ShouldCreateNewSemester()
    {
        // Arrange
        var cache =  CreateMemoryCache();
        await using var context = CreateContext(cache);
        await ClearDatabase(context);
        
        var command = CreateCommand(context, cache);
        var caller = DefaultTeacherEntity();
        var validSemesterName = "2022-2023/spring";
        var payload = new StartNewSemesterCommandPayload
        {
            SemesterName = validSemesterName
        };
        
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);
        var semester = await context.Semesters.FirstOrDefaultAsync(s => s.Name == validSemesterName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(semester);
        Assert.Equal(semester.Name, validSemesterName);
    }

    [Fact]
    public async Task StartNewSemesterAsync_InvalidName_ShouldReturnSemesterNameValidationException()
    {
        // Arrange
        var cache =  CreateMemoryCache();
        await using var context = CreateContext(cache);
        await ClearDatabase(context);
        
        var command = CreateCommand(context, cache);
        var caller = DefaultTeacherEntity();
        var validSemesterName = "invalid_name";
        var payload = new StartNewSemesterCommandPayload
        {
            SemesterName = validSemesterName
        };
        
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<SemesterNameValidationException>(exception);
            return true;
        });
    }
    
    private StartNewSemesterCommand CreateCommand(ApplicationContext context, IMemoryCache cache)
    {
        return new StartNewSemesterCommand(context, cache);
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