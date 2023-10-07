using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Tests.Setup.Utils.Comparers;

public static class ArchiveStudentComparer
{
    public static bool Compare(ArchivedStudentEntity first, ArchivedStudentEntity second)
    {
        return first.StudentGuid == second.StudentGuid &&
               first.SemesterName == second.SemesterName &&
               first.FullName == second.FullName &&
               first.GroupNumber == second.GroupNumber &&
               DoubleComparer.Compare(first.TotalPoints, second.TotalPoints) &&
               first.Visits == second.Visits;
    }
}