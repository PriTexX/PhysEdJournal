using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class AssignCuratorCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task AssignCuratorAsync_WithExistingGroupAndTeacher_ShouldAssignCuratorToGroup()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var teacher = DefaultTeacherEntity();
        var group = DefaultGroupEntity();
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = group.GroupName, 
            TeacherGuid = teacher.TeacherGuid
        };
        
        context.Teachers.Add(caller);
        context.Teachers.Add(teacher);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);
        group = await context.Groups.FindAsync(group.GroupName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(teacher.TeacherGuid, group?.CuratorGuid);
    }

    [Fact]
    public async Task AssignCuratorAsync_WithNonExistingGroup_ShouldReturnGroupNotFoundException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var teacher = DefaultTeacherEntity();
        var groupName = "non-existing-group";
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = groupName, 
            TeacherGuid = teacher.TeacherGuid
        };

        context.Teachers.Add(caller);
        context.Teachers.Add(teacher);
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
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var teacher = DefaultTeacherEntity();
        var group = DefaultGroupEntity();
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = group.GroupName, 
            TeacherGuid = teacher.TeacherGuid
        };
        
        context.Teachers.Add(caller);
        context.Groups.Add(group);
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

    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    private AssignCuratorCommand CreateCommand(ApplicationContext context)
    {
        return new AssignCuratorCommand(context);
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