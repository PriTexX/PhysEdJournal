// using PhysEdJournal.Core.Entities.Types;
// using PhysEdJournal.Core.Exceptions.TeacherExceptions;
// using PhysEdJournal.Infrastructure.Commands.AdminCommands;
// using PhysEdJournal.Tests.Setup;
// using PhysEdJournal.Tests.Setup.Utils;
//
// namespace PhysEdJournal.Tests.Tests.Commands.Admin;
//
// public sealed class CreateTeacherCommandTests : DatabaseTestsHelper
// {
//     [Fact]
//     public async Task CreateTeacherAsync_WhenNewTeacher_ShouldCreateTeacher()
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new CreateTeacherCommand(context);
//         var payload = new CreateTeacherCommandPayload
//         {
//             TeacherGuid = "Default",
//             FullName = "Default",
//             Permissions = TeacherPermissions.DefaultAccess,
//         };
//
//         // Act
//         var result = await command.ExecuteAsync(payload);
//
//         // Assert
//         Assert.True(result.IsOk);
//         await using var assertContext = CreateContext();
//         var teacherFromDb = await assertContext.Teachers.FindAsync(payload.TeacherGuid);
//         Assert.NotNull(teacherFromDb);
//         Assert.Equal(payload.TeacherGuid, teacherFromDb.TeacherGuid);
//     }
//
//     [Fact]
//     public async Task CreateTeacherAsync_WhenDuplicateTeacher_ShouldThrow()
//     {
//         // Arrange
//         await using var context = CreateContext();
//         await ClearDatabase(context);
//
//         var command = new CreateTeacherCommand(context);
//         var teacher = EntitiesFactory.CreateTeacher(TeacherPermissions.DefaultAccess);
//         var payload = new CreateTeacherCommandPayload
//         {
//             TeacherGuid = teacher.TeacherGuid,
//             FullName = teacher.FullName,
//             Permissions = teacher.Permissions,
//         };
//
//         await context.Teachers.AddAsync(teacher);
//         await context.SaveChangesAsync();
//
//         // Act
//         var result = await command.ExecuteAsync(payload);
//
//         // Assert
//         Assert.False(result.IsOk);
//         result.Match(
//             _ => true,
//             exception =>
//             {
//                 Assert.IsType<TeacherAlreadyExistsException>(exception);
//                 return true;
//             }
//         );
//     }
// }
