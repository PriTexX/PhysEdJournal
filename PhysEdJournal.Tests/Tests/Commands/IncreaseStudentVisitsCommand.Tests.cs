using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class IncreaseStudentVisitsCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task IncreaseVisitsAsync_IncreasesStudentsTotalVisits_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = student.StudentGuid,
            Date = DateOnlyGenerator.GetWorkingDate(),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.Equal(studentFromDb.Visits, student.Visits);
    }

    [Fact]
    public async Task IncreaseVisitsAsync_StudentNotFoundException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = "Default",
            Date = DateOnlyGenerator.GetWorkingDate(),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<StudentNotFoundException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task IncreaseVisitsAsync_ActionFromFutureException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = student.StudentGuid,
            Date = DateOnlyGenerator.GetWorkingDate(DateOnly.MaxValue),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<ActionFromFutureException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task IncreaseVisitsAsync_VisitExpiredException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = student.StudentGuid,
            Date = DateOnlyGenerator.GetWorkingDate(DateOnly.MinValue.AddDays(2)),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<VisitExpiredException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task IncreaseVisitsAsync_VisitAlreadyExistsException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = student.StudentGuid,
            Date = DateOnlyGenerator.GetWorkingDate(),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var prevRes = await command.ExecuteAsync(payload);
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(prevRes.IsSuccess);
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<VisitAlreadyExistsException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task IncreaseVisitsAsync_NonWorkingDayException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new IncreaseStudentVisitsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var payload = new IncreaseStudentVisitsCommandPayload
        {
            StudentGuid = student.StudentGuid,
            Date = DateOnlyGenerator.GetNonWorkingDate(),
            TeacherGuid = teacher.TeacherGuid,
            IsAdmin = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<NonWorkingDayException>(exception);
                return true;
            }
        );
    }
}
