using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class AssignCuratorCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task AssignCuratorAsync_WithExistingGroupAndTeacher_ShouldAssignCuratorToGroup()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new AssignCuratorCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(TeacherPermissions.DefaultAccess);
        var group = EntitiesFactory.CreateGroup("Default");
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = group.GroupName, 
            TeacherGuid = teacher.TeacherGuid
        };
        
        await context.Teachers.AddAsync(teacher);
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        group = await assertContext.Groups.FindAsync(group.GroupName);
        Assert.NotNull(group);
        Assert.Equal(teacher.TeacherGuid, group.CuratorGuid);
    }

    [Fact]
    public async Task AssignCuratorAsync_WithNonExistingGroup_ShouldReturnGroupNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new AssignCuratorCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(TeacherPermissions.DefaultAccess);
        var groupName = "non-existing-group";
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = groupName, 
            TeacherGuid = teacher.TeacherGuid
        };
        
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<GroupNotFoundException>(exception);
            return true;
        });
    }
    
    [Fact]
    public async Task AssignCuratorAsync_WithNonExistingTeacher_ShouldReturnTeacherNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new AssignCuratorCommand(context);
        var group = EntitiesFactory.CreateGroup("Default");
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = group.GroupName, 
            TeacherGuid = "teacher.TeacherGuid"
        };
        
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<TeacherNotFoundException>(exception);
            return true;
        });
    }
}