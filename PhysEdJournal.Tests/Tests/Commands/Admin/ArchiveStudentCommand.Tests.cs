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

        var payload = new ArchiveStudentPayload { StudentGuid = student.StudentGuid, };

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
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var archivedStudentFromDb = await assertContext.ArchivedStudents.FindAsync(
            student.StudentGuid,
            lastSemester.Name
        );
        var activeStudent = await assertContext
            .Students.Where(s => s.StudentGuid == student.StudentGuid)
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

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public async Task ArchiveStudentAsync_MarksOldDebt_ShouldWorkProperly(
        bool hasDebt,
        bool usedToHaveDebt,
        bool expected
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
            hasDebt,
            true,
            51
        );
        student.HadDebtInSemester = usedToHaveDebt;

        var payload = new ArchiveStudentPayload { StudentGuid = student.StudentGuid, };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.True(result.IsOk);
        await using var assertContext = CreateContext();
        var activeStudent = await assertContext
            .Students.Where(s => s.StudentGuid == student.StudentGuid)
            .Include(s => s.PointsStudentHistory)
            .Include(s => s.VisitsStudentHistory)
            .Include(s => s.StandardsStudentHistory)
            .FirstOrDefaultAsync();

        Assert.Equal(expected, student.HadDebtInSemester);
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

        var payload = new ArchiveStudentPayload { StudentGuid = student.StudentGuid, };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsOk);
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

        var payload = new ArchiveStudentPayload { StudentGuid = "student.StudentGuid ", };

        await context.Semesters.AddAsync(lastSemester);
        await context.Semesters.AddAsync(currentSemester);
        await context.Groups.AddAsync(group);
        await context.Teachers.AddAsync(teacher);
        await context.SaveChangesAsync();

        //Act
        var result = await command.ExecuteAsync(payload);

        //Assert
        Assert.False(result.IsOk);
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
