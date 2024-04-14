using Microsoft.Extensions.Logging;
using Moq;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Jobs;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;
using Quartz;

namespace PhysEdJournal.Tests.Tests.Services.Quartz.Jobs;

public sealed class ArchiveStudentJobTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    public async Task ArchiveStudentAsync_ArchivesStudent_ShouldWorkProperly(int additionalPoints)
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var loggerMock = new Mock<ILogger<ArchiveStudentJob>>();
        var jobContextMock = new Mock<IJobExecutionContext>();
        Exception? possibleException = null;

        var command = new ArchiveStudentCommand(context);
        var job = new ArchiveStudentJob(context, command, loggerMock.Object);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var lastSemester = EntitiesFactory.CreateSemester("2022-2023/spring", false);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", 2, teacher.TeacherGuid);
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            lastSemester.Name,
            true,
            true,
            additionalPoints
        );

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        try
        {
            await job.Execute(jobContextMock.Object);
        }
        catch (Exception e)
        {
            possibleException = e;
        }

        //Assert
        Assert.Null(possibleException);
        await using var assertContext = CreateContext();
        var archivedStudentFromDb = await assertContext.ArchivedStudents.FindAsync(
            student.StudentGuid,
            lastSemester.Name
        );
        Assert.NotNull(archivedStudentFromDb);
        Assert.Equal(archivedStudentFromDb.StudentGuid, student.StudentGuid);
    }
}
