using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class DeletePointsCommandTests : DatabaseTestsHelper
{
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
    [InlineData(100)]
    public async Task DeletePointsAsync_DeletePointsFromStudent_ShouldWorkProperly(
        int additionalPoints
    )
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedAdditionalPoints = additionalPoints - 10;

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            additionalPoints
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.PointsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = false,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.PointsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedAdditionalPoints, studentFromDb.AdditionalPoints);
        var historyEntityFromDb = studentFromDb.PointsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeletePointsAsync_DeletePointsFromStudentForce_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        const int expectedAdditionalPoints = 0;

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            10
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.PointsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );

        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = true,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.PointsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedAdditionalPoints, studentFromDb.AdditionalPoints);
        var historyEntityFromDb = studentFromDb.PointsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeletePointsAsync_PointsOutdatedException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            10
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-32);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.PointsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );

        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = false,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<PointsOutdatedException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeletePointsAsync_PointsStudentHistoryNotFoundException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            10
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = 0,
            IsAdmin = false,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<PointsStudentHistoryNotFoundException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeletePointsAsync_TeacherGuidMismatchException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        const int expectedAdditionalPoints = 0;

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            10
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.PointsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );

        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = "historyEntity.TeacherGuid",
            HistoryId = historyObj!.Id,
            IsAdmin = false,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<TeacherGuidMismatchException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeletePointsAsync_TeacherGuidMismatchButWithForce_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        const int expectedAdditionalPoints = 0;

        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            semester.Name,
            false,
            true,
            10
        );
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Activist,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.PointsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );

        var payload = new DeletePointsCommandPayload
        {
            TeacherGuid = "historyEntity.TeacherGuid",
            HistoryId = historyObj!.Id,
            IsAdmin = true,
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.PointsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedAdditionalPoints, studentFromDb.AdditionalPoints);
        var historyEntityFromDb = studentFromDb.PointsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }
}
