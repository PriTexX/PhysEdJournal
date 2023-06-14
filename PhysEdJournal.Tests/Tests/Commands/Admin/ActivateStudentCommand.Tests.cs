using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Infrastructure.Commands.AdminCommands;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Tests.Setup;

namespace PhysEdJournal.Tests.Tests.Commands.Admin;

public sealed class ActivateStudentCommandTests : DatabaseTestsHelper
{
    [Fact]
    public async Task ActivateStudentAsync_WhenStudentExists_ShouldActivateStudent()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var semester = DefaultSemesterEntity();
        var group = DefaultGroupEntity("211-729");
        var student = DefaultStudentEntity();

        await context.Semesters.AddAsync(semester);
        await context.Groups.AddAsync(group);
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        // Act
        var result = await command.ExecuteAsync(student.StudentGuid);
        var studentFromDb = await context.Students.FindAsync(student.StudentGuid);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(studentFromDb);
        Assert.True(studentFromDb.IsActive);
    }
    
    [Fact]
    public async Task ActivateStudentAsync_WithoutStudent_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        await ClearDatabase(context);
        
        var command = CreateCommand(context);
        var student = DefaultStudentEntity();

        // Act
        var result = await command.ExecuteAsync(student.StudentGuid);

        // Assert
        Assert.False(result.IsSuccess);
    }

    private StudentEntity DefaultStudentEntity(bool hasDebt = false, bool isActive = false)
    {
        var student = new StudentEntity
        {
            StudentGuid = Guid.NewGuid().ToString(),
            FullName = "John Smith",
            GroupNumber = "211-729",
            HasDebtFromPreviousSemester = hasDebt,
            ArchivedVisitValue = 10.5,
            AdditionalPoints = 2,
            Visits = 10,
            Course = 2,
            HealthGroup = HealthGroupType.Basic,
            Department = "IT",
            CurrentSemesterName = "2022-2023/spring",
            IsActive = isActive,
            PointsForStandards = 2,
        };

        return student;
    }

    private SemesterEntity DefaultSemesterEntity(string semesterName = "2022-2023/spring", bool isCurrent = true)
    {
        var semester = new SemesterEntity { Name = semesterName, IsCurrent = isCurrent };

        return semester;
    }
    
    private GroupEntity DefaultGroupEntity(string groupName = "DefaultName")
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }

    private ActivateStudentCommand CreateCommand(ApplicationContext context)
    {
        return new ActivateStudentCommand(context);
    }
    
    private TeacherEntity DefaultTeacherEntity(TeacherPermissions permissions = TeacherPermissions.DefaultAccess)
    {
        var teacher = new TeacherEntity()
        {
            FullName = "DefaultName",
            TeacherGuid = Guid.NewGuid().ToString(),
            Permissions = permissions
        };
        return teacher;
    }
}