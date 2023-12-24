using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Tests.Setup;
using PhysEdJournal.Tests.Setup.Utils;
using PhysEdJournal.Tests.Setup.Utils.Comparers;

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
        var pointsEntity = EntitiesFactory.CreatePointsStudentHistoryEntity(
            student.StudentGuid,
            WorkType.Science,
            teacher.TeacherGuid,
            DateOnly.FromDateTime(DateTime.Now),
            10
        );
        var visitsEntity = EntitiesFactory.CreateVisitStudentHistoryEntity(
            DateOnly.FromDateTime(DateTime.Now),
            teacher.TeacherGuid,
            student.StudentGuid
        );
        var standardsEntity = EntitiesFactory.CreateStandardsHistoryEntity(
            student.StudentGuid,
            StandardType.Jumps,
            teacher.TeacherGuid,
            DateOnly.FromDateTime(DateTime.Now),
            4
        );

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            SemesterName = currentSemester.Name,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddAsync(pointsEntity);
        await context.VisitsStudentsHistory.AddAsync(visitsEntity);
        await context.StandardsStudentsHistory.AddAsync(standardsEntity);
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
        var activeStudent = await assertContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .FirstOrDefaultAsync();

        Assert.NotNull(archivedStudentFromDb);
        Assert.NotNull(activeStudent);
        Assert.Equal(0, student.ArchivedVisitValue);
        Assert.False(student.HasDebtFromPreviousSemester);
        Assert.Equal(activeStudent.StudentGuid, archivedStudentFromDb.StudentGuid);

        Assert.Empty(activeStudent.PointsStudentHistory!);
        Assert.Empty(activeStudent.StandardsStudentHistory!);
        Assert.Empty(activeStudent.VisitsStudentHistory!);

        Assert.NotNull(archivedStudentFromDb.VisitsHistory);
        Assert.NotNull(archivedStudentFromDb.StandardsHistory);
        Assert.NotNull(archivedStudentFromDb.PointsHistory);

        Assert.NotEmpty(archivedStudentFromDb.VisitsHistory);
        Assert.NotEmpty(archivedStudentFromDb.StandardsHistory);
        Assert.NotEmpty(archivedStudentFromDb.PointsHistory);

        Assert.True(
            ArchivedHistoryComparer.ComparePointsRecordWithArchivedPointsRecord(
                pointsEntity,
                archivedStudentFromDb.PointsHistory[0]
            )
        );
        Assert.True(
            ArchivedHistoryComparer.CompareStandardsRecordWithArchivedStandardsRecord(
                standardsEntity,
                archivedStudentFromDb.StandardsHistory[0]
            )
        );
        Assert.True(
            ArchivedHistoryComparer.CompareVisitsRecordWithArchivedHistoryRecord(
                visitsEntity,
                archivedStudentFromDb.VisitsHistory[0]
            )
        );
    }

    [Fact]
    public async Task ArchiveStudentAsync_TransfersPoints_ShouldWorkProperly()
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
            true,
            true,
            51
        );
        student.ArchivedVisitValue = 1;
        student.Visits = 1;

        var points = CreatePointsHistory(student.StudentGuid, teacher.TeacherGuid, 51);

        var visits = CreateVisitsHistory(student.StudentGuid, teacher.TeacherGuid, 1);

        var standards = CreateStandardsHistory(student.StudentGuid, teacher.TeacherGuid, 1);

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            SemesterName = currentSemester.Name,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddRangeAsync(points);
        await context.VisitsStudentsHistory.AddRangeAsync(visits);
        await context.StandardsStudentsHistory.AddRangeAsync(standards);
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
        var activeStudent = await assertContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .FirstOrDefaultAsync();

        Assert.NotNull(archivedStudentFromDb);
        Assert.NotNull(activeStudent);
        Assert.Equal(activeStudent.StudentGuid, archivedStudentFromDb.StudentGuid);

        Assert.Equal(1, student.PointsStudentHistory?.Count);
        Assert.Equal(1, student.StandardsStudentHistory?.Count);
        Assert.Equal(1, student.VisitsStudentHistory?.Count);
    }

    [Fact]
    public async Task ArchiveStudentAsync_NoTransfersPointsWhenHasNoDebt_ShouldWorkProperly()
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
            51
        );
        student.ArchivedVisitValue = 1;
        student.Visits = 1;

        var points = CreatePointsHistory(student.StudentGuid, teacher.TeacherGuid, 51);

        var visits = CreateVisitsHistory(student.StudentGuid, teacher.TeacherGuid, 1);

        var standards = CreateStandardsHistory(student.StudentGuid, teacher.TeacherGuid, 1);

        var payload = new ArchiveStudentCommandPayload
        {
            StudentGuid = student.StudentGuid,
            SemesterName = currentSemester.Name,
        };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.PointsStudentsHistory.AddRangeAsync(points);
        await context.VisitsStudentsHistory.AddRangeAsync(visits);
        await context.StandardsStudentsHistory.AddRangeAsync(standards);
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
        var activeStudent = await assertContext.Students
            .Where(s => s.StudentGuid == student.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .FirstOrDefaultAsync();

        Assert.NotNull(archivedStudentFromDb);
        Assert.NotNull(activeStudent);
        Assert.Equal(activeStudent.StudentGuid, archivedStudentFromDb.StudentGuid);

        Assert.Equal(0, student.PointsStudentHistory?.Count);
        Assert.Equal(0, student.StandardsStudentHistory?.Count);
        Assert.Equal(0, student.VisitsStudentHistory?.Count);
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
            SemesterName = currentSemester.Name,
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
            SemesterName = currentSemester.Name
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

    /// <summary>
    /// Метод генерирует массив историй баллов для тестов архиваций.
    /// По умолчанию берем минимально возможные значения для очков == 1, так как это упростит тестирование.
    /// </summary>
    /// <param name="studentGuid"></param>
    /// <param name="teacherGuid"></param>
    /// <param name="recordsCount"></param>
    /// <returns></returns>
    private IEnumerable<PointsStudentHistoryEntity> CreatePointsHistory(
        string studentGuid,
        string teacherGuid,
        int recordsCount
    )
    {
        var history = new List<PointsStudentHistoryEntity>();

        while (recordsCount > 0)
        {
            history.Add(
                EntitiesFactory.CreatePointsStudentHistoryEntity(
                    studentGuid,
                    WorkType.Science,
                    teacherGuid,
                    DateOnly.FromDateTime(DateTime.Now),
                    1
                )
            );
            recordsCount--;
        }

        return history;
    }

    /// <summary>
    /// Метод генерирует массив историй посещений для тестов архиваций.
    /// </summary>
    /// <param name="studentGuid"></param>
    /// <param name="teacherGuid"></param>
    /// <param name="recordsCount"></param>
    /// <returns></returns>
    private IEnumerable<VisitStudentHistoryEntity> CreateVisitsHistory(
        string studentGuid,
        string teacherGuid,
        int recordsCount
    )
    {
        var history = new List<VisitStudentHistoryEntity>();

        while (recordsCount > 0)
        {
            history.Add(
                EntitiesFactory.CreateVisitStudentHistoryEntity(
                    DateOnly.FromDateTime(DateTime.Now),
                    teacherGuid,
                    studentGuid
                )
            );
            recordsCount--;
        }

        return history;
    }

    /// <summary>
    /// Метод генерирует массив историй нормативов для тестов архиваций.
    /// По умолчанию берем минимально возможные значения для очков == 1, так как это упростит тестирование.
    /// </summary>
    /// <param name="studentGuid"></param>
    /// <param name="teacherGuid"></param>
    /// <param name="recordsCount"></param>
    /// <returns></returns>
    private IEnumerable<StandardsStudentHistoryEntity> CreateStandardsHistory(
        string studentGuid,
        string teacherGuid,
        int recordsCount
    )
    {
        var history = new List<StandardsStudentHistoryEntity>();

        while (recordsCount > 0)
        {
            history.Add(
                EntitiesFactory.CreateStandardsHistoryEntity(
                    studentGuid,
                    StandardType.Jumps,
                    teacherGuid,
                    DateOnly.FromDateTime(DateTime.Now),
                    1
                )
            );
            recordsCount--;
        }

        return history;
    }
}
