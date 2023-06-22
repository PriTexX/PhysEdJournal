using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class CreateCompetitionCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task CreateCompetitionAsync_WhenNewCompetition_ShouldCreateCompetition()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new CreateCompetitionCommand(context);
        var competitionName = "прыжки";

        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(competitionName);
        var competition = await context.Competitions.FindAsync(competitionName);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(competition);
        Assert.Equal(competitionName, competition.CompetitionName);
    }
}