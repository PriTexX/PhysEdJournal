using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
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
        
        var command = CreateCommand(context);
        var competitionName = "прыжки";
        var caller = DefaultTeacherEntity();
        var payload = new CreateCompetitionCommandPayload
        {
            CompetitionName = competitionName
        };

        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
        var competition = await context.Competitions.FindAsync(competitionName);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(competition);
        Assert.Equal(competitionName, competition.CompetitionName);
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

    private CreateCompetitionCommand CreateCommand(ApplicationContext context)
    {
        return new CreateCompetitionCommand(context);
    }
}