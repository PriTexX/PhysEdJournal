using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using PhysEdJournal.Infrastructure.Validators.Permissions;

namespace PhysEdJournal.Tests;

public class TeacherServiceTests
{
    private readonly DbContextOptions<ApplicationContext> _contextOptions;

    public TeacherServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Theory]
    [InlineData(TeacherPermissions.AdminAccess)]
    [InlineData(TeacherPermissions.DefaultAccess)]
    [InlineData(TeacherPermissions.SecretaryAccess)] 
    [InlineData(TeacherPermissions.OnlineCourseAccess)]
    public async Task GivePermissionsAsync_WhenValidInput_ShouldChangeTeacherPermissions(TeacherPermissions permissions)
    {
         // Arrange
        var context = CreateContext();
        var teacherService = CreateTeacherService(context);
        var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
        var expectedPermissions = permissions;
        var teacher = DefaultTeacherEntity(permissions);
        await context.Teachers.AddAsync(caller);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

         // Act
        var result = await teacherService.GivePermissionsAsync(caller.TeacherGuid, teacher.TeacherGuid, permissions);

         // Assert
        Assert.True(result.IsSuccess);

        var teacherFromDb = await context.Teachers.FindAsync(teacher.TeacherGuid);
        Assert.Equal(expectedPermissions, teacherFromDb?.Permissions);
     }
     
     [Fact]
     public async Task GivePermissionsAsync_WhenSuperUser_ShouldChangeTeacherPermissions()
     {
         // Arrange
         TeacherPermissions permissions = TeacherPermissions.SuperUser;
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         var teacher = DefaultTeacherEntity(permissions);
         await context.Teachers.AddAsync(caller);
         await context.Teachers.AddAsync(teacher);
         await context.SaveChangesAsync();

         // Act
         var result = await teacherService.GivePermissionsAsync(caller.TeacherGuid, teacher.TeacherGuid, permissions);

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
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         var teacher = DefaultTeacherEntity(permissions);
         await context.Teachers.AddAsync(caller);
         await context.SaveChangesAsync();
     
         // Act
         var result = await teacherService.GivePermissionsAsync(caller.TeacherGuid, teacher.TeacherGuid, permissions);
     
         // Assert
         Assert.False(result.IsSuccess);
         result.Match(_ => true, exception =>
         {
             Assert.IsType<TeacherNotFoundException>(exception);
             return true;
         });
     }
     
     [Fact]
     public async Task CreateCompetitionAsync_WhenNewCompetition_ShouldCreateCompetition()
     {
         // Arrange
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var competitionName = "прыжки";
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         await context.Teachers.AddAsync(caller);
         await context.SaveChangesAsync();

         // Act
         var result = await teacherService.CreateCompetitionAsync(caller.TeacherGuid, competitionName);

         // Assert
         Assert.True(result.IsSuccess);
         var competition = await context.Competitions.FindAsync(competitionName);
         Assert.NotNull(competition);
         Assert.Equal(competitionName, competition.CompetitionName);
     }
     
     [Fact]
     public async Task CreateCompetitionAsync_WhenDuplicateCompetition_ShouldThrow()
     {
         // Arrange
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var competitionName = "прыжки";
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         var competition = new CompetitionEntity {CompetitionName = competitionName};
         await context.Competitions.AddAsync(competition);
         await context.Teachers.AddAsync(caller);
         await context.SaveChangesAsync();

         // Act
         var result = await teacherService.CreateCompetitionAsync(caller.TeacherGuid, competitionName);

         // Assert
         Assert.False(result.IsSuccess);
         result.Match(_ => true, exception =>
         {
             Assert.IsType<InvalidOperationException>(exception);
             return true;
         });
     }
     
     [Fact]
     public async Task DeleteCompetitionAsync_WhenCompetitionExists_ShouldDeleteCompetition()
     {
         // Arrange
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var competitionName = "прыжки";
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         var competition = new CompetitionEntity {CompetitionName = competitionName};
         await context.Competitions.AddAsync(competition);
         await context.Teachers.AddAsync(caller);
         await context.SaveChangesAsync();

         // Act
         var result = await teacherService.DeleteCompetitionAsync(caller.TeacherGuid, competitionName);

         // Assert
         Assert.True(result.IsSuccess);
         var competitionFromDb = await context.Competitions.FindAsync(competitionName);
         Assert.Null(competitionFromDb);
     }
     
     [Fact]
     public async Task DeleteCompetitionAsync_WhenNoCompetition_ShouldThrow()
     {
         // Arrange
         var context = CreateContext();
         var teacherService = CreateTeacherService(context);
         var competitionName = "прыжки";
         var caller = new TeacherEntity {TeacherGuid = Guid.NewGuid().ToString(), FullName = "caller", Permissions = TeacherPermissions.SuperUser};
         await context.Teachers.AddAsync(caller);
         await context.SaveChangesAsync();

         // Act
         var result = await teacherService.DeleteCompetitionAsync(caller.TeacherGuid, competitionName);

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
     
     private MemoryCache CreateMemoryCache()
     {
         return new MemoryCache(new MemoryCacheOptions());
     }
     
     private ApplicationContext CreateContext()
     {
         return new ApplicationContext(_contextOptions, CreateMemoryCache());
     }

     private TeacherService CreateTeacherService(ApplicationContext context)
     {
         var teacherService = new TeacherService(context, new PermissionValidator(context, CreateMemoryCache()), CreateMemoryCache());

         return teacherService;
     }
}