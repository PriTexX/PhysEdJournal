﻿using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class DeleteCompetitionCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task DeleteCompetitionAsync_WhenCompetitionExists_ShouldDeleteCompetition()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var competitionName = "прыжки";
        var caller = DefaultTeacherEntity();
        var competition = new CompetitionEntity {CompetitionName = competitionName};
        var payload = new DeleteCompetitionCommandPayload
        {
            CompetitionName = competitionName
        };

        await context.Competitions.AddAsync(competition);
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
        var competitionFromDb = await context.Competitions.FindAsync(competitionName);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(competitionFromDb);
    }
     
    [Fact]
    public async Task DeleteCompetitionAsync_WhenNoCompetition_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var competitionName = "прыжки";
        var caller = DefaultTeacherEntity();
        var payload = new DeleteCompetitionCommandPayload
        {
            CompetitionName = competitionName
        };
        
        await context.Teachers.AddAsync(caller);
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<CompetitionNotFoundException>(exception);
            return true;
        });
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

    private DeleteCompetitionCommand CreateCommand(ApplicationContext context)
    {
        return new DeleteCompetitionCommand(context);
    }
}