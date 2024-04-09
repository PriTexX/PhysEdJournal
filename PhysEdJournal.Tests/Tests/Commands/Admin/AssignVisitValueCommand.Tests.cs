using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

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

        var command = new AssignVisitValueCommand(context);
        var group = EntitiesFactory.CreateGroup("Default");
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = group.GroupName,
            NewVisitValue = visitValue
        };

        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var groupFromDb = await assertContext.Groups.FindAsync(group.GroupName);
        Assert.NotNull(groupFromDb);
        Assert.Equal(visitValue, groupFromDb.VisitValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.MinValue)]
    public async Task AssignVisitValueAsync_WithInvalidValue_ShouldReturnNullVisitValueException(
        double visitValue
    )
    {
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AssignVisitValueCommand(context);
        var group = EntitiesFactory.CreateGroup("Default");
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = group.GroupName,
            NewVisitValue = visitValue
        };

        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<NullVisitValueException>(exception);
                return true;
            }
        );
    }

    [Theory]
    [InlineData(5)]
    [InlineData(double.MaxValue)]
    [InlineData(4)]
    public async Task AssignVisitValueAsync_WithoutGroup_ShouldReturnGroupNotFoundException(
        double visitValue
    )
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AssignVisitValueCommand(context);
        var payload = new AssignVisitValueCommandPayload
        {
            GroupName = "default",
            NewVisitValue = visitValue
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<GroupNotFoundException>(exception);
                return true;
            }
        );
    }
}
