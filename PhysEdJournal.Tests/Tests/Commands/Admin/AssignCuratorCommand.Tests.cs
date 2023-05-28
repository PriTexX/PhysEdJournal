using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
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
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var teacher = new TeacherEntity { TeacherGuid = Guid.NewGuid().ToString(), FullName = "teacher"};
        var group = DefaultGroupEntity();
        context.Teachers.Add(caller);
        context.Teachers.Add(teacher);
        context.Groups.Add(group);
        await context.SaveChangesAsync();
        var payload = new AssignCuratorCommandPayload
        {
            GroupName = group.GroupName, 
            TeacherGuid = teacher.TeacherGuid
        };

        // Act
        var result = await command.ExecuteAsync(payload);
        group = await context.Groups.FindAsync(group.GroupName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(teacher.TeacherGuid, group?.CuratorGuid);
    }

    // [Fact]
    // public async Task AssignCuratorAsync_WithNonExistingGroup_ShouldReturnGroupNotFoundException()
    // {
    //     // Arrange
    //     var context = CreateContext();
    //     var groupService = CreateCommand(context);
    //     var caller = new TeacherEntity() {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
    //     var teacher = new TeacherEntity { TeacherGuid = Guid.NewGuid().ToString(), FullName = "Teacher1"};
    //     var groupName = "non-existing-group";
    //
    //     context.Teachers.Add(caller);
    //     context.Teachers.Add(teacher);
    //     await context.SaveChangesAsync();
    //
    //     // Act
    //     var result = await groupService.AssignCuratorAsync(groupName, teacher.TeacherGuid);
    //
    //     // Assert
    //     Assert.False(result.IsSuccess);
    //     result.Match(_ => true, exception =>
    //     {
    //         Assert.IsType<GroupNotFoundException>(exception);
    //         return true;
    //     });
    // }
    //
    // [Fact]
    // public async Task AssignCuratorAsync_WithNonExistingTeacher_ShouldReturnTeacherNotFoundException()
    // {
    //     // Arrange
    //     var context = CreateContext();
    //     var groupService = CreateCommand(context);
    //     var caller = new TeacherEntity() {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
    //     var teacherGuid = Guid.NewGuid().ToString();
    //     var group = DefaultGroupEntity();
    //     context.Teachers.Add(caller);
    //     context.Groups.Add(group);
    //     await context.SaveChangesAsync();
    //
    //     // Act
    //     var result = await groupService.AssignCuratorAsync(group.GroupName, teacherGuid);
    //
    //     // Assert
    //     Assert.False(result.IsSuccess);
    //     result.Match(_ => true, exception =>
    //     {
    //         Assert.IsType<TeacherNotFoundException>(exception);
    //         return true;
    //     });
    // }
    
    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    private AssignCuratorCommand CreateCommand(ApplicationContext context)
    {
        return new AssignCuratorCommand(context);
    }
}