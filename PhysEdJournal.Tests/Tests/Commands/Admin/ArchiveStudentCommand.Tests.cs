using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;
using PhysEdJournal.Tests.Setup.Utils.Comparers;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class ArchiveStudentCommandTests : DatabaseTestsHelper
{
    [Theory]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    public async Task ArchiveStudentAsync_ArchivesStudent_ShouldWorkProperly(int additionalPoints)
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new ArchiveStudentCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var lastSemester = EntitiesFactory.CreateSemester("2022-2023/spring", false);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", 2, teacher.TeacherGuid);
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            lastSemester.Name,
            false,
            true,
            additionalPoints
        );

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            SemesterName = lastSemester.Name,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = CalculateTotalPoints(
                student.Visits,
                group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ),
            Visits = student.Visits
        };

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            IsForceMode = false,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var archivedStudentFromDb = await assertContext.ArchivedStudents.FindAsync(
            student.StudentGuid,
            lastSemester.Name
        );
        Assert.NotNull(archivedStudentFromDb);
        Assert.True(ArchiveStudentComparer.Compare(archivedStudentFromDb, archivedStudent));
    }

    [Theory]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    [InlineData(49)]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public async Task ArchiveStudentAsync_ArchivesStudentForceMode_ShouldWorkProperly(
        int additionalPoints
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new ArchiveStudentCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var lastSemester = EntitiesFactory.CreateSemester("2022-2023/spring", false);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", 2, teacher.TeacherGuid);
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            lastSemester.Name,
            false,
            true,
            additionalPoints
        );

        var archivedStudent = new ArchivedStudentEntity
        {
            StudentGuid = student.StudentGuid,
            SemesterName = lastSemester.Name,
            FullName = student.FullName,
            GroupNumber = student.GroupNumber,
            TotalPoints = CalculateTotalPoints(
                student.Visits,
                group.VisitValue,
                student.AdditionalPoints,
                student.PointsForStandards
            ),
            Visits = student.Visits
        };

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            IsForceMode = true,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(result.IsSuccess);
        await using var assertContext = CreateContext();
        var archivedStudentFromDb = await assertContext.ArchivedStudents.FindAsync(
            student.StudentGuid,
            lastSemester.Name
        );
        Assert.NotNull(archivedStudentFromDb);
        Assert.True(ArchiveStudentComparer.Compare(archivedStudentFromDb, archivedStudent));
    }

    [Theory]
    [InlineData(49)]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public async Task ArchiveStudentAsync_NotEnoughPointsException_ShouldThrowException(
        int additionalPoints
    )
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var visitValue = 4;
        var command = new ArchiveStudentCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var lastSemester = EntitiesFactory.CreateSemester("2022-2023/spring", false);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", visitValue, teacher.TeacherGuid);
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            lastSemester.Name,
            false,
            true,
            additionalPoints
        );

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            IsForceMode = false,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
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
                Assert.IsType<NotEnoughPointsException>(exception);
                return true;
            }
        );
        await using var assertContext = CreateContext();
        var studentFromDb = await assertContext.Students.FindAsync(student.StudentGuid);
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.HasDebtFromPreviousSemester);
        Assert.Equal(visitValue, studentFromDb.ArchivedVisitValue);
    }

    [Fact]
    public async Task ArchiveStudentAsync_StudentNotFoundException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new ArchiveStudentCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var lastSemester = EntitiesFactory.CreateSemester("2022-2023/spring", false);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", 2, teacher.TeacherGuid);

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = "student.StudentGuid ",
            IsForceMode = false,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
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
    public async Task ArchiveStudentAsync_CannotMigrateToNewSemesterException_ShouldThrowException()
    {
        //Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);

        var command = new ArchiveStudentCommand(context);
        var teacher = EntitiesFactory.CreateTeacher(permissions: TeacherPermissions.SuperUser);
        var currentSemester = EntitiesFactory.CreateSemester("2021-2023/autumn", true);
        var group = EntitiesFactory.CreateGroup("211-729", 2, teacher.TeacherGuid);
        var student = EntitiesFactory.CreateStudent(
            group.GroupName,
            currentSemester.Name,
            false,
            true,
            50
        );

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            IsForceMode = false,
        };

        await context.Semesters.AddAsync(currentSemester);
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
                Assert.IsType<CannotMigrateToNewSemesterException>(exception);
                return true;
            }
        );
    }
}
