using DB.Tables;

namespace GraphQL.Api.QueryExtensions;

[ExtendObjectType(nameof(TeacherEntity))]
public class TeacherQueryExtensions
{
    [BindMember(nameof(TeacherEntity.Permissions))]
    public IEnumerable<string> GetPermissions([Parent] TeacherEntity teacher)
    {
        return teacher.Permissions.ToString().Split(",").AsEnumerable().Select(s => s.Trim());
    }
}
