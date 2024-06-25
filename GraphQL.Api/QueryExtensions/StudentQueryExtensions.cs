using DB.Tables;

namespace GraphQL.Api.QueryExtensions;

[ExtendObjectType(typeof(StudentEntity))]
public class StudentQueryExtensions
{
    [BindMember(nameof(StudentEntity.PointsHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<PointsHistoryEntity> GetPointsHistory([Parent] StudentEntity student)
    {
        return student.PointsHistory!;
    }

    [BindMember(nameof(StudentEntity.VisitsHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<VisitsHistoryEntity> GetVisitsHistory([Parent] StudentEntity student)
    {
        return student.VisitsHistory!;
    }

    [BindMember(nameof(StudentEntity.StandardsHistory))]
    [UseFiltering]
    [UseSorting]
    public IEnumerable<StandardsHistoryEntity> GetStandardsHistory([Parent] StudentEntity student)
    {
        return student.StandardsHistory!;
    }
}
