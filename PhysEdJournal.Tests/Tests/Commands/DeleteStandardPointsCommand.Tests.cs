using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.PointsExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class DeleteStandardPointsCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(10, 0)]
    [InlineData(100, 0)]
    public async Task DeleteStandardPointsAsync_DeletesPointsForStandardsFromStudent_ShouldWorkProperly(
        int standardPoints,
        int expectedStandardPoints
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = standardPoints;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = false
        };

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
        Assert.Equal(expectedStandardPoints, studentFromDb.PointsForStandards);
        var historyEntityFromDb = studentFromDb.StandardsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeleteStandardPointsAsync_DeletesProperRecord_ShouldWorkProperly()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 30;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );
        var historyEntity1 = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            20
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.StandardsStudentsHistory.AddAsync(historyEntity1);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = false
        };

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
        Assert.Equal(historyEntity1.Points, studentFromDb.PointsForStandards);
        var historyEntityFromDb = studentFromDb.StandardsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeleteStandardPointsAsync_DeletesPointsFromStudentForce_ShouldWorkProperly()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedStandardPoints = 0;

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 10;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = true
        };

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
        Assert.Equal(expectedStandardPoints, studentFromDb.PointsForStandards);
        var historyEntityFromDb = studentFromDb.StandardsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeleteStandardPointsAsync_PointsOutdatedException_ShouldThrow()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 10;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-32);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(date),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = historyObj!.Id,
            IsAdmin = false
        };

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsSuccess);
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
    public async Task DeleteStandardPointsAsync_StandardsStudentHistoryNotFoundException_ShouldThrow()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 10;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            HistoryId = 0,
            IsAdmin = false
        };

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsSuccess);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<StandardsStudentHistoryNotFoundException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeleteStandardPointsAsync_TeacherGuidMismatchException_ShouldThrow()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 10;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = "historyEntity.TeacherGuid",
            HistoryId = historyObj!.Id,
            IsAdmin = false
        };

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsSuccess);
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
    public async Task DeleteStandardPointsAsync_TeacherGuidMismatchButWithForce_ShouldWorkProperly()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedStandardPoints = 0;

        var command = new DeleteStandardPointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.PointsForStandards = 10;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnlyGenerator.GetWorkingDate(),
            10
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.StandardsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.StandardsStudentsHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        var payload = new DeleteStandardPointsCommandPayload
        {
            TeacherGuid = "historyEntity.TeacherGuid",
            HistoryId = historyObj!.Id,
            IsAdmin = true
        };

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
        Assert.Equal(expectedStandardPoints, studentFromDb.PointsForStandards);
        var historyEntityFromDb = studentFromDb.StandardsStudentHistory.FirstOrDefault(
            h => h.Points == historyEntity.Points
        );
        Assert.Null(historyEntityFromDb);
    }
}
