using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests;

public class GroupServiceTests
{
    private readonly ApplicationContext _context;
    private readonly GroupService _groupService;

    public GroupServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase("Guid.NewGuid().ToString()")
            .Options;
        _context = new ApplicationContext(options);

        _groupService = new GroupService(_context, Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = null,
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0
        }));
    }

    [Fact]
    public async Task AssignCuratorAsync_WithExistingGroupAndTeacher_ShouldAssignCuratorToGroup()
    {
        // Arrange
        var teacher = new TeacherEntity() { TeacherGuid = Guid.NewGuid().ToString(), FullName = "teacher"};
        var group = new GroupEntity() { GroupName = "test-group" };
        _context.Teachers.Add(teacher);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _groupService.AssignCuratorAsync(group.GroupName, teacher.TeacherGuid);
        group = await _context.Groups.FindAsync(group.GroupName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(teacher.TeacherGuid, group?.CuratorGuid);
    }

    [Fact]
    public async Task AssignCuratorAsync_WithNonExistingGroup_ShouldReturnGroupNotFoundException()
    {
        // Arrange
        var teacher = new TeacherEntity() { TeacherGuid = Guid.NewGuid().ToString(), FullName = "Teacher1"};
        var groupName = "non-existing-group";
        
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        // Act
        var result = await _groupService.AssignCuratorAsync(groupName, teacher.TeacherGuid);

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
        var teacherGuid = Guid.NewGuid().ToString();
        var group = new GroupEntity() { GroupName = "test-group" };
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _groupService.AssignCuratorAsync(group.GroupName, teacherGuid);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<GroupNotFoundException>(exception);
            return true;
        });
    }
}