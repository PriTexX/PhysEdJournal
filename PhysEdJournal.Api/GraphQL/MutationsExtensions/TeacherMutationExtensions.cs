using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Api.GraphQL.MutationsExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TeacherMutationExtensions
{
    public async Task<TeacherEntity> CreateTeacherAsync([ID] Guid teacherGuid, string fullName, [Service] ITeacherService teacherService)
    {
        var result = await teacherService.CreateTeacherAsync(new TeacherEntity
        {
            TeacherGuid = teacherGuid.ToString(),
            FullName = fullName,
            Permissions = TeacherPermissions.DefaultAccess
        });

        return result.Match(teacher => new TeacherEntity{FullName = fullName, TeacherGuid = teacherGuid.ToString(), Permissions = TeacherPermissions.DefaultAccess}, exception => throw exception);
    }
}