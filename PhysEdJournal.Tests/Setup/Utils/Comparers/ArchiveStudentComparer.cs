using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Tests.Setup.Utils.Comparers;

public static class ArchiveStudentComparer
{
    public static bool Compare(ArchivedStudentEntity first, ArchivedStudentEntity second)
    {
        if (!CompareHistories(first.PointsHistory, second.PointsHistory))
        {
            return false;
        }

        if (!CompareHistories(first.VisitsHistory, second.VisitsHistory))
        {
            return false;
        }

        if (!CompareHistories(first.StandardsHistory, second.StandardsHistory))
        {
            return false;
        }

        return first.StudentGuid == second.StudentGuid
            && first.SemesterName == second.SemesterName
            && first.FullName == second.FullName
            && first.GroupNumber == second.GroupNumber
            && first.Visits == second.Visits;
    }

    private static bool CompareHistories<T>(IList<T> first, IList<T> second)
        where T : ArchivedHistory
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        for (var i = 0; i < first.Count; i++)
        {
            if (CompareHistoryRecords(first[i], second[i]))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool CompareHistoryRecords<T>(T first, T second)
        where T : ArchivedHistory
    {
        if (first.StudentGuid != second.StudentGuid)
        {
            return false;
        }

        if (first.TeacherGuid != second.TeacherGuid)
        {
            return false;
        }

        if (!DoubleComparer.Compare(first.Points, second.Points))
        {
            return false;
        }

        if (first.Date != second.Date)
        {
            return false;
        }

        return true;
    }
}
