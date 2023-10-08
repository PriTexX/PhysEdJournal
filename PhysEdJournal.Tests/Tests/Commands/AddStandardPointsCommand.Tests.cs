using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class AddStandardPointsCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(StandardType.Jumps)]
    [InlineData(StandardType.Squats)]
    [InlineData(StandardType.Tilts)]
    [InlineData(StandardType.PullUps)]
    [InlineData(StandardType.TorsoLifts)]
    [InlineData(StandardType.ShuttleRun)]
    [InlineData(StandardType.JumpingRopeJumps)]
    [InlineData(StandardType.FlexionAndExtensionOfArms)]
    public async Task AddPointsForStandardsAsync_AddsPointsForDifferentStandardsToStudent_ShouldWorkProperly(
        StandardType standardType
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            standardType,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.StandardsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.NotNull(
            studentFromDb.StandardsStudentHistory.FirstOrDefault(
                h => h.Points == historyEntity.Points
            )
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public async Task AddPointsForStandardsAsync_AddsProperPointsForStandardToStudent_ShouldWorkProperly(
        int points
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            points
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.StandardsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.NotNull(
            studentFromDb.StandardsStudentHistory.FirstOrDefault(
                h => h.Points == historyEntity.Points
            )
        );
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(12)]
    public async Task AddPointsForStandardsAsync_PointsOverflowException_ShouldThrowException(
        int points
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            points
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<PointsOverflowException>(exception);
                return true;
            }
        );
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task AddPointsForStandardsAsync_NegativePointsValueException_ShouldThrowException(
        int points
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            points
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<NegativePointAmount>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task AddPointsForStandardsAsync_ActionFromFutureException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(DateOnly.MaxValue),
            10
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
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
    public async Task AddPointsForStandardsAsync_StudentNotFoundException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            "wrong",
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
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
    public async Task AddPointsForStandardsAsync_StandardAlreadyExistsException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            8
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var firstTry = await command.ExecuteAsync(payload);
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(firstTry.IsSuccess);
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<StandardAlreadyExistsException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task AddPointsForStandardsAsync_OverridesStandardPoints_ShouldWorkProperly()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            8
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };
        var payload2 = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points + 2,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = true
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var firstTry = await command.ExecuteAsync(payload);
        var overrideTry = await command.ExecuteAsync(payload2);

        //Assert
        Assert.True(firstTry.IsSuccess);
        Assert.True(overrideTry.IsSuccess);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.StandardsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.NotNull(
            studentFromDb.StandardsStudentHistory.FirstOrDefault(h => h.Points == payload2.Points)
        );
        Assert.Equal(2, studentFromDb.StandardsStudentHistory.Count);
    }

    [Fact]
    public async Task AddPointsForStandardsAsync_LoweringTheScoreException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            8
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
        };
        var payload2 = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points - 2,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = true
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var firstTry = await command.ExecuteAsync(payload);
        var overrideTry = await command.ExecuteAsync(payload2);

        //Assert
        Assert.True(firstTry.IsSuccess);
        Assert.False(overrideTry.IsSuccess);
        overrideTry.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<LoweringTheScoreException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task AddPointsForStandardsAsync_PointsExpiredException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var oldDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-31);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(oldDate),
            10
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
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
                Assert.IsType<DateExpiredException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task AddPointsAsync_NonWorkingDayException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Tilts,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetNonWorkingDate(),
            10
        );
        var payload = new AddStandardPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            StandardType = StandardType.Tilts,
            IsOverride = false
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
