using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Api.GraphQL.QueryExtensions;

[ExtendObjectType(typeof(StudentEntity))]
public class StudentQueryExtensions
{
    [BindMember(nameof(StudentEntity.PointsStudentHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<PointsStudentHistoryEntity> GetPointsHistory([Parent] StudentEntity student)
    {
        return student.PointsStudentHistory!;
    }

    [BindMember(nameof(StudentEntity.VisitsStudentHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<VisitStudentHistoryEntity> GetVisitsHistory([Parent] StudentEntity student)
    {
        return student.VisitsStudentHistory!;
    }

    [BindMember(nameof(StudentEntity.StandardsStudentHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<StandardsStudentHistoryEntity> GetStandardsHistory(
        [Parent] StudentEntity student
    )
    {
        return student.StandardsStudentHistory!;
    }
}
