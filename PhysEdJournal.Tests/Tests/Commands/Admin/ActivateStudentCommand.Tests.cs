using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class ActivateStudentCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ActivateStudentAsync_WhenStudentExists_ShouldActivateStudent(bool isActive)
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new ActivateStudentCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, isActive);

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(student.StudentGuid);

        // Assert
        Assert.True(result.IsSuccess);
        var studentFromDb = await context.Students
            .AsNoTracking()
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.IsActive);
    }
    
    [Fact]
    public async Task ActivateStudentAsync_WithoutStudent_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new ActivateStudentCommand(context);

        // Act
        var result = await command.ExecuteAsync("student.StudentGuid");

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<StudentNotFoundException>(exception);
            return true;
        });
    }
}