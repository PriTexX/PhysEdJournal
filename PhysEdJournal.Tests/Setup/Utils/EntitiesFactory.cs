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
    
    public static StudentEntity CreateStudent(string groupNumber, string currentSemesterName, bool hasDebtFromPreviousSemester, bool isActive, int additionalPoints)
    {
        return new StudentEntity
        {
            StudentGuid = Guid.NewGuid().ToString(),
            FullName = "John Smith",
            GroupNumber = groupNumber,
            HasDebtFromPreviousSemester = hasDebtFromPreviousSemester,
            ArchivedVisitValue = 0,
            AdditionalPoints = additionalPoints,
            Visits = 0,
            Course = 2,
            HealthGroup = HealthGroupType.Basic,
            Department = "IT",
            CurrentSemesterName = currentSemesterName, //"2022-2023/spring"
            IsActive = isActive,
            PointsForStandards = 0,
        };
    }

    public static SemesterEntity CreateSemester(string semesterName, bool isCurrent)
    {
        var semester = new SemesterEntity { Name = semesterName, IsCurrent = isCurrent };

        return semester;
    }
    
    public static GroupEntity CreateGroup(string groupName)
    {
        var group = new GroupEntity {GroupName = groupName};

        return group;
    }
    public static GroupEntity CreateGroup(string groupName, double visitValue, string curatorGuid)
    {
        var group = new GroupEntity
        {
            GroupName = groupName,
            VisitValue = visitValue,
            CuratorGuid = curatorGuid
        };

        return group;
    }
    
    public static TeacherEntity CreateTeacher(TeacherPermissions permissions)
    {
        var teacher = new TeacherEntity
        {
            FullName = "DefaultName",
            TeacherGuid = Guid.NewGuid().ToString(),
            Permissions = permissions
        };
        return teacher;
    }
    
    public static PointsStudentHistoryEntity CreatePointsStudentHistoryEntity( string studentGuid, WorkType workType, string teacherGuid, DateOnly date, int points)
    {
        var historyEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            Date = date == default
                ? DateOnly.FromDateTime(DateTime.Today)
                : date,
            TeacherGuid = teacherGuid,
            WorkType = workType,
            Points = points,
        };

        return historyEntity;
    }
    
    public static StandardsStudentHistoryEntity CreateStandardsHistoryEntity(string studentGuid, StandardType standardType, string teacherGuid, DateOnly date, int points)
    {
        var historyEntity = new StandardsStudentHistoryEntity
        {
            StudentGuid = studentGuid,
            Date = date == default ? DateOnly.FromDateTime(DateTime.Today) : date,
            TeacherGuid = teacherGuid,
            StandardType = standardType,
            Points = points,
        };

        return historyEntity;
    }
}