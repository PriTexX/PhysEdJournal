using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Tests.Tests.Services;

public class GroupServiceTests
{
    private readonly DbContextOptions<ApplicationContext> _contextOptions;

    public GroupServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AssignCuratorAsync_WithExistingGroupAndTeacher_ShouldAssignCuratorToGroup()
    {
        // Arrange
        var context = CreateContext();
        var groupService = CreateGroupService(context);
        var caller = new TeacherEntity() {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var teacher = new TeacherEntity { TeacherGuid = Guid.NewGuid().ToString(), FullName = "teacher"};
        var group = DefaultGroupEntity();
        context.Teachers.Add(caller);
        context.Teachers.Add(teacher);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await groupService.AssignCuratorAsync(group.GroupName, teacher.TeacherGuid);
        group = await context.Groups.FindAsync(group.GroupName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(teacher.TeacherGuid, group?.CuratorGuid);
    }

    [Fact]
    public async Task AssignCuratorAsync_WithNonExistingGroup_ShouldReturnGroupNotFoundException()
    {
        // Arrange
        var context = CreateContext();
        var groupService = CreateGroupService(context);
        var caller = new TeacherEntity() {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var teacher = new TeacherEntity { TeacherGuid = Guid.NewGuid().ToString(), FullName = "Teacher1"};
        var groupName = "non-existing-group";

        context.Teachers.Add(caller);
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await groupService.AssignCuratorAsync(groupName, teacher.TeacherGuid);

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
        var context = CreateContext();
        var groupService = CreateGroupService(context);
        var caller = new TeacherEntity() {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var teacherGuid = Guid.NewGuid().ToString();
        var group = DefaultGroupEntity();
        context.Teachers.Add(caller);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await groupService.AssignCuratorAsync(group.GroupName, teacherGuid);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<TeacherNotFoundException>(exception);
            return true;
        });
    }
    
    [Theory]
    [InlineData(5)]
    [InlineData(double.MaxValue)]
    [InlineData(4)]
    public async Task AssignVisitValueAsync_WithValidValue_ShouldAssignVisitValue(double visitValue)
    {
        // Arrange
        var context = CreateContext();
        var groupService = CreateGroupService(context);
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var group = DefaultGroupEntity();
        context.Teachers.Add(caller);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await groupService.AssignVisitValueAsync(group.GroupName, visitValue);

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
        // Arrange
        var context = CreateContext();
        var groupService = CreateGroupService(context);
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var group = DefaultGroupEntity();
        context.Teachers.Add(caller);
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        // Act
        var result = await groupService.AssignVisitValueAsync(group.GroupName, visitValue);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<NullVisitValueException>(exception);
            return true;
        });
    }

    private ApplicationContext CreateContext()
    {
        return new ApplicationContext(_contextOptions, CreateMemoryCache());
    }
    
    private MemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    private GroupService CreateGroupService(ApplicationContext context)
    {
        return new GroupService(context, Options.Create(new ApplicationOptions
        {
            UserInfoServerURL = null,
            PageSizeToQueryUserInfoServer = 0,
            PointBorderForSemester = 0,
            RsaPublicKey = null
        }));
    }
}