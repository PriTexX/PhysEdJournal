using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class AssignVisitValueCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(5)]
    [InlineData(double.MaxValue)]
    [InlineData(4)]
    public async Task AssignVisitValueAsync_WithValidValue_ShouldAssignVisitValue(double visitValue)
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var group = DefaultGroupEntity();
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = group.GroupName,
            NewVisitValue = visitValue
        };

        context.Teachers.Add(caller);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsSuccess);
        var groupFromDb = await context.Groups.FindAsync(group.GroupName);
        Assert.NotNull(groupFromDb);
        Assert.Equal(visitValue, groupFromDb.VisitValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.MinValue)]
    public async Task AssignVisitValueAsync_WithInvalidValue_ShouldReturnNullVisitValueException(double visitValue)
    {
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var group = DefaultGroupEntity();
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = group.GroupName,
            NewVisitValue = visitValue
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
            Assert.IsType<NullVisitValueException>(exception);
            return true;
        });
    }
    
    [Theory]
    [InlineData(5)]
    [InlineData(double.MaxValue)]
    [InlineData(4)]
    public async Task AssignVisitValueAsync_WithoutGroup_ShouldReturnGroupNotFoundException(double visitValue)
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var caller = DefaultTeacherEntity();
        var group = DefaultGroupEntity();
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = group.GroupName,
            NewVisitValue = visitValue
        };

        context.Teachers.Add(caller);
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
    
    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    private AssignVisitValueCommand CreateCommand(ApplicationContext context)
    {
        return new AssignVisitValueCommand(context);
    }
    
    private TeacherEntity DefaultTeacherEntity(TeacherPermissions permissions = TeacherPermissions.DefaultAccess)
    {
        var teacher = new TeacherEntity
        {
            FullName = "DefaultName",
            TeacherGuid = Guid.NewGuid().ToString(),
            Permissions = permissions
        };
        return teacher;
    }
}