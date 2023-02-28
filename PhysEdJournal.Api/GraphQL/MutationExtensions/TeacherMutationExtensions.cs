using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TeacherMutationExtensions
{
    public async Task<TeacherEntity> CreateTeacherAsync(string teacherGuid, string fullName, [Service] ITeacherService teacherService)
    {
        var result = await teacherService.CreateTeacherAsync(new TeacherEntity
        {
            TeacherGuid = teacherGuid.ToString(),
            FullName = fullName,
            Permissions = TeacherPermissions.DefaultAccess
        });

        return result.Match(_ => new TeacherEntity{FullName = fullName, TeacherGuid = teacherGuid.ToString(), Permissions = TeacherPermissions.DefaultAccess}, exception => throw exception);
    }

    public async Task<Success> UpdateTeachersInfoAsync([Service] ITeacherService teacherService)
    {
        var result = await teacherService.UpdateTeacherInfoAsync();
        
        return result.Match(_ => true, exception => throw exception);
    }

    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> GivePermissionsToTeacherAsync(string teacherGuid, IEnumerable<TeacherPermissions> permissions, [Service] ITeacherService teacherService)
    {
        var teacherPermissions = permissions.Aggregate((prev, next) => prev | next);
        var result = await teacherService.GivePermissionsAsync(teacherGuid, teacherPermissions);
        
        return result.Match(_ => true, exception => throw exception);
    }
}