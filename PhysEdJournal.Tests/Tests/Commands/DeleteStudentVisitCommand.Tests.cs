using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Core.Exceptions.VisitsExceptions;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class DeleteStudentVisitCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task DeleteVisitAsync_DeleteStudentsVisit_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedVisits = 0;

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.VisitsStudentsHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            HistoryId = historyObj.Id,
            IsAdmin = false
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.VisitsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedVisits, studentFromDb.Visits);
        var historyEntityFromDb = studentFromDb.VisitsStudentHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeleteVisitAsync_DeleteStudentsVisitForce_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedVisits = 0;

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(date),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.VisitsStudentsHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            HistoryId = historyObj.Id,
            IsAdmin = true
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.VisitsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedVisits, studentFromDb.Visits);
        var historyEntityFromDb = studentFromDb.VisitsStudentHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        Assert.Null(historyEntityFromDb);
    }

    [Fact]
    public async Task DeleteVisitAsync_VisitOutdatedException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-8);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(date),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.VisitsStudentsHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            HistoryId = historyObj.Id,
            IsAdmin = false
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<VisitOutdatedException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeleteVisitAsync_VisitsStudentHistoryNotFoundException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = teacher.TeacherGuid,
            HistoryId = 0,
            IsAdmin = false
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.False(result.IsOk);
        result.Match(
            _ => true,
            exception =>
            {
                Assert.IsType<VisitsStudentHistoryNotFoundException>(exception);
                return true;
            }
        );
    }

    [Fact]
    public async Task DeleteVisitPointsAsync_TeacherGuidMismatchException_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.VisitsStudentsHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = "teacher.TeacherGuid",
            HistoryId = historyObj.Id,
            IsAdmin = false
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
    public async Task DeleteVisitPointsAsync_TeacherGuidMismatchButWithForce_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var expectedVisits = 0;

        var command = new DeleteStudentVisitCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        student.Visits = 1;
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnlyGenerator.GetWorkingDate(),
            teacher.TeacherGuid,
            student.StudentGuid
        );

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.VisitsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        var historyObj = context.VisitsStudentsHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        var payload = new DeleteStudentVisitCommandPayload
        {
            TeacherGuid = "teacher.TeacherGuid",
            HistoryId = historyObj.Id,
            IsAdmin = true
        };

        // Act
        var result = await command.ExecuteAsync(payload);

        // Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students
            .Include(s => s.VisitsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid)
            .FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        Assert.Equal(expectedVisits, studentFromDb.Visits);
        var historyEntityFromDb = studentFromDb.VisitsStudentHistory.FirstOrDefault(
            h => h.Date == historyEntity.Date
        );
        Assert.Null(historyEntityFromDb);
    }
}
