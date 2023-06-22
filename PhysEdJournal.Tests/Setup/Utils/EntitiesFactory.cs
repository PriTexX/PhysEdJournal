using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Tests.Setup.Utils;

public static class EntitiesFactory
{
    public static StudentEntity CreateStudent(string groupNumber, string currentSemesterName, bool hasDebtFromPreviousSemester, bool isActive)
    {
        return new StudentEntity
        {
            StudentGuid = Guid.NewGuid().ToString(),
            FullName = "John Smith",
            GroupNumber = groupNumber,
            HasDebtFromPreviousSemester = hasDebtFromPreviousSemester,
            ArchivedVisitValue = 10.5,
            AdditionalPoints = 2,
            Visits = 10,
            Course = 2,
            HealthGroup = HealthGroupType.Basic,
            Department = "IT",
            CurrentSemesterName = currentSemesterName, //"2022-2023/spring"
            IsActive = isActive,
            PointsForStandards = 2,
        };
    }
    
    public static SemesterEntity DefaultSemesterEntity(string semesterName, bool isCurrent)
    {
        var semester = new SemesterEntity { Name = semesterName, IsCurrent = isCurrent };

        return semester;
    }
    
    public static GroupEntity DefaultGroupEntity(string groupName)
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    
    public static TeacherEntity DefaultTeacherEntity(TeacherPermissions permissions)
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