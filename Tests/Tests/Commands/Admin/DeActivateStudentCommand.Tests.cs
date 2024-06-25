// using PhysEdJournal.Core.Exceptions.StudentExceptions;
// using PhysEdJournal.Infrastructure.Commands.AdminCommands;
// using Tests.Setup;
// using Tests.Setup.Utils;
//
// namespace Tests.Tests.Commands.Admin;
//
// public sealed class DeActivateStudentCommandTests : DatabaseTestsHelper
// {
//     [Theory]
//     [InlineData(true)]
//     [InlineData(false)]
//     public async Task DeActivateStudentAsync_WhenStudentExists_ShouldDeActivateStudent(
//         bool isActive
//     )
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new DeActivateStudentCommand(context);
//         var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
//         var group = EntitiesFactory.CreateGroup("211-729");
//         var student = EntitiesFactory.CreateStudent(
//             group.GroupName,
//             semester.Name,
//             false,
//             isActive
//         );
//
//         await context.Semesters.AddAsync(semester);
//         await context.Groups.AddAsync(group);
//         await context.Students.AddAsync(student);
//         await context.SaveChangesAsync();
//
//         // Act
//         var result = await command.ExecuteAsync(student.StudentGuid);
//
//         // Assert
//         Assert.True(result.IsOk);
//         await using var assertContext = CreateContext();
//         var studentFromDb = await assertContext.Students.FindAsync(student.StudentGuid);
//         Assert.NotNull(studentFromDb);
//         Assert.False(studentFromDb.IsActive);
//     }
//
//     [Fact]
//     public async Task DeActivateStudentAsync_WithoutStudent_ShouldThrow()
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new DeActivateStudentCommand(context);
//         var student = EntitiesFactory.CreateStudent("211-729", "2022-2023/spring", false, false);
//
//         // Act
//         var result = await command.ExecuteAsync(student.StudentGuid);
//
//         // Assert
//         Assert.False(result.IsOk);
//         result.Match(
//             _ => true,
//             exception =>
//             {
//                 Assert.IsType<StudentNotFoundException>(exception);
//                 return true;
//             }
//         );
//     }
// }
