using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class GivePermissionsCommandTests  : DatabaseTestsHelper
{
    [Theory]
    [InlineData(TeacherPermissions.AdminAccess)]
    [InlineData(TeacherPermissions.DefaultAccess)]
    [InlineData(TeacherPermissions.SecretaryAccess)] 
    [InlineData(TeacherPermissions.OnlineCourseAccess)]
    public async Task GivePermissionsAsync_WhenValidInput_ShouldChangeTeacherPermissions(TeacherPermissions permissions)
    {
         // Arrange
         var cache =  CreateMemoryCache();
         await using var context = CreateContext(cache);
         await ClearDatabase(context);
         
         var command = new GivePermissionsCommand(context, cache);
         var expectedPermissions = permissions; 
         var teacher = EntitiesFactory.CreateTeacher(TeacherPermissions.DefaultAccess);
         var payload = new GivePermissionsCommandPayload
         {
             TeacherGuid = teacher.TeacherGuid,
             TeacherPermissions = permissions
         };
         
         await context.Teachers.AddAsync(teacher); 
         await context.SaveChangesAsync();
    
         // Act
        var result = await command.ExecuteAsync(payload);
        var teacherFromDb = await context.Teachers.FindAsync(teacher.TeacherGuid);
    
         // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPermissions, teacherFromDb?.Permissions);
     }
     
    [Fact]
    public async Task GivePermissionsAsync_WhenSuperUser_ShouldChangeTeacherPermissions()
    {
        // Arrange
        var cache =  CreateMemoryCache();
        await using var context = CreateContext(cache);
        await ClearDatabase(context);
        
        var command = new GivePermissionsCommand(context, cache);
        var permissions = TeacherPermissions.SuperUser;
        var teacher = EntitiesFactory.CreateTeacher(TeacherPermissions.DefaultAccess);
        var payload = new GivePermissionsCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            TeacherPermissions = permissions
        };
        
        await context.Teachers.AddAsync(teacher); 
        await context.SaveChangesAsync();
    
        // Act
        var result = await command.ExecuteAsync(payload);
    
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<CannotGrantSuperUserPermissionsException>(exception);
            return true;
        });
    }
    
    [Theory]
    [InlineData(TeacherPermissions.AdminAccess)]
    [InlineData(TeacherPermissions.DefaultAccess)]
    [InlineData(TeacherPermissions.SecretaryAccess)] 
    [InlineData(TeacherPermissions.OnlineCourseAccess)]
    public async Task GivePermissionsAsync_WhenTeacherNotExists_ShouldReturnTeacherNotFound(TeacherPermissions permissions)
    {
        // Arrange
        var cache =  CreateMemoryCache();
        await using var context = CreateContext(cache);
        await ClearDatabase(context);
         
        var command = new GivePermissionsCommand(context, cache);
        var payload = new GivePermissionsCommandPayload
        {
            TeacherGuid = "Default",
            TeacherPermissions = permissions
        };

        // Act
        var result = await command.ExecuteAsync(payload);
        
        // Assert
        Assert.False(result.IsSuccess);
        result.Match(_ => true, exception =>
        {
            Assert.IsType<TeacherNotFoundException>(exception);
            return true;
        });
    }
}