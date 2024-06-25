// using PhysEdJournal.Core.Entities.DB;
// using PhysEdJournal.Core.Exceptions.TeacherExceptions;
// using PhysEdJournal.Infrastructure.Commands.AdminCommands;
// using Tests.Setup;
//
// namespace Tests.Tests.Commands.Admin;
//
// public sealed class DeleteCompetitionCommandTests : DatabaseTestsHelper
// {
//     [Fact]
//     public async Task DeleteCompetitionAsync_WhenCompetitionExists_ShouldDeleteCompetition()
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new DeleteCompetitionCommand(context);
//         var competitionName = "прыжки";
//         var competition = new CompetitionEntity { CompetitionName = competitionName };
//
//         await context.Competitions.AddAsync(competition);
//         await context.SaveChangesAsync();
//
//         // Act
//         var result = await command.ExecuteAsync(competitionName);
//
//         // Assert
//         Assert.True(result.IsOk);
//         await using var assertContext = CreateContext();
//         var competitionFromDb = await assertContext.Competitions.FindAsync(competitionName);
//         Assert.Null(competitionFromDb);
//     }
//
//     [Fact]
//     public async Task DeleteCompetitionAsync_WhenNoCompetition_ShouldThrow()
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new DeleteCompetitionCommand(context);
//         var competitionName = "прыжки";
//
//         await context.SaveChangesAsync();
//
//         // Act
//         var result = await command.ExecuteAsync(competitionName);
//
//         // Assert
//         Assert.False(result.IsOk);
//         result.Match(
//             _ => true,
//             exception =>
//             {
//                 Assert.IsType<CompetitionNotFoundException>(exception);
//                 return true;
//             }
//         );
//     }
// }
