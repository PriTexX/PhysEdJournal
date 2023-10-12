using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;

namespace PhysEdJournal.Tests.Tests.Commands;

public sealed class DeletePointsCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task DeletePointsAsync_DeletePointsFromStudent_ShouldWorkProperly()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = new DeletePointsCommand(context);
        var semester = EntitiesFactory.CreateSemester("2022-2023/spring", true);
        var group = EntitiesFactory.CreateGroup("211-729");
        var student = EntitiesFactory.CreateStudent(group.GroupName, semester.Name, false, true);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var historyEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(student.StudentGuid, WorkType.Activist, teacher.TeacherGuid, DateOnlyGenerator.GetWorkingDate(), 10, semester.Name);
        var payload = new DeletePointsCommandPayload
        {
            StudentGuid = historyEntity.StudentGuid,
            TeacherGuid = historyEntity.TeacherGuid,
            HistoryId = 1,
            IgnoreDateIntervalCheck = false,
        };

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student); 
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(historyEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(payload);
        
        // Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students.Include(s => s.PointsStudentHistory)
            .Where(s => s.StudentGuid == student.StudentGuid).FirstOrDefaultAsync();
        Assert.NotNull(studentFromDb);
        var duplicate = studentFromDb.PointsStudentHistory.FirstOrDefault(h => h.Points == historyEntity.Points);
        Assert.Null(duplicate);
    }
}