using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class StartNewSemesterCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task StartNewSemesterAsync_ValidName_ShouldCreateNewSemester()
    {
        // Arrange
        var cache = CreateMemoryCache();
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new StartNewSemesterCommand(context);
        var validSemesterName = "2022-2023/spring";

        // Act
        var result = await command.ExecuteAsync(validSemesterName);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var semester = await assertContext.Semesters.FindAsync(validSemesterName);
        Assert.NotNull(semester);
        Assert.Equal(semester.Name, validSemesterName);
    }

    [Fact]
    public async Task StartNewSemesterAsync_InvalidName_ShouldReturnSemesterNameValidationException()
    {
        // Arrange
        var cache = CreateMemoryCache();
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new StartNewSemesterCommand(context);
        var invalidSemesterName = "invalid_name";

        // Act
        var result = await command.ExecuteAsync(invalidSemesterName);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<SemesterNameValidationException>(exception);
                return true;
            }
        );
    }
}
