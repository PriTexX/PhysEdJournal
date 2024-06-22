// using PhysEdJournal.Core.Entities.DB;
//
// namespace Tests.Setup.Utils.Comparers;
//
// public static class ArchivedHistoryComparer
// {
//     public static bool ComparePointsRecordWithArchivedPointsRecord(
//         PointsStudentHistoryEntity first,
//         ArchivedPointsHistory second
//     )
//     {
//         return first.StudentGuid == second.StudentGuid
//             && first.TeacherGuid == second.TeacherGuid
//             && first.Date == second.Date
//             && DoubleComparer.Compare(first.Points, second.Points)
//             && first.WorkType == second.WorkType
//             && first.Comment == second.Comment;
//     }
//
//     public static bool CompareStandardsRecordWithArchivedStandardsRecord(
//         StandardsStudentHistoryEntity first,
//         ArchivedStandardsHistory second
//     )
//     {
//         return first.StudentGuid == second.StudentGuid
//             && first.TeacherGuid == second.TeacherGuid
//             && first.Date == second.Date
//             && DoubleComparer.Compare(first.Points, second.Points)
//             && first.Comment == second.Comment
//             && first.StandardType == second.StandardType;
//     }
//
//     public static bool CompareVisitsRecordWithArchivedHistoryRecord(
//         VisitStudentHistoryEntity first,
//         ArchivedHistory second
//     )
//     {
//         return first.StudentGuid == second.StudentGuid
//             && first.TeacherGuid == second.TeacherGuid
//             && first.Date == second.Date;
//     }
// }
