using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class AddPointsCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(WorkType.OnlineWork)]
    [InlineData(WorkType.ExternalFitness)]
    [InlineData(WorkType.Activist)]
    [InlineData(WorkType.Science)]
    [InlineData(WorkType.GTO)]
    [InlineData(WorkType.InternalTeam)]
    [InlineData(WorkType.Competition)]
    public async Task AddPointsAsync_AddsPointsToStudent_ShouldWorkProperly(WorkType workType)
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            workType,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );
        var payload = new AddPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            WorkType = historyEntity.WorkType
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
        var studentFromDb = await assertContext.Students
            .Include(s => s.PointsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        var duplicate = studentFromDb.PointsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.NotNull(duplicate);
    }

    [Theory]
    [InlineData(WorkType.OnlineWork)]
    [InlineData(WorkType.ExternalFitness)]
    [InlineData(WorkType.Activist)]
    [InlineData(WorkType.Science)]
    [InlineData(WorkType.GTO)]
    [InlineData(WorkType.InternalTeam)]
    [InlineData(WorkType.Competition)]
    public async Task AddPointsAsync_ActionFromFutureException_ShouldThrowException(
        WorkType workType
    )
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            workType,
            teacher.TeacherGuid,
            DateOnly.MaxValue,
            10
        );
        var payload = new AddPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            WorkType = historyEntity.WorkType
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
    public async Task AddPointsAsync_StudentNotFoundException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            "wrongGuid",
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );
        var payload = new AddPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            WorkType = historyEntity.WorkType
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
    public async Task AddPointsAsync_PointsExpiredException_ShouldThrowException()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new AddPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var oldDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-31);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.OnlineWork,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(oldDate),
            10
        );
        var payload = new AddPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            WorkType = historyEntity.WorkType
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

        var command = new AddPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetNonWorkingDate(),
            10
        );
        var payload = new AddPointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            Date = historyEntity.Date,
            Points = historyEntity.Points,
            TeacherGuid = historyEntity.TeacherGuid,
            WorkType = historyEntity.WorkType
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
