using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class CreateTeacherCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task CreateTeacherAsync_WhenNewTeacher_ShouldCreateTeacher()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
            
        var command = CreateCommand(context);
        var teacher = DefaultTeacherEntity();
        var payload = new CreateTeacherCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            FullName = teacher.FullName,
            Permissions = TeacherPermissions.DefaultAccess,
            Groups = null
        };

        // Act
        var result = await command.ExecuteAsync(payload);
        var teacherFromDb = await context.Teachers.FindAsync(teacher.TeacherGuid);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(teacherFromDb);
        Assert.Equal(teacher.TeacherGuid, teacherFromDb.TeacherGuid);
    }
     
    [Fact]
    public async Task CreateTeacherAsync_WhenDuplicateTeacher_ShouldThrow()
    { 
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
            
        var command = CreateCommand(context);
        var teacher = DefaultTeacherEntity();
        var payload = new CreateTeacherCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            FullName = teacher.FullName,
            Permissions = TeacherPermissions.DefaultAccess,
            Groups = null
        };

        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

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

    private CreateTeacherCommand CreateCommand(ApplicationContext context)
    {
        return new CreateTeacherCommand(context);
    }
}