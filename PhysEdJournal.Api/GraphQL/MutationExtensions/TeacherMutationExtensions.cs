using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TeacherMutationExtensions
{
    
    [Error(typeof(TeacherAlreadyExistsException))]
    public async Task<TeacherEntity> CreateTeacherAsync(string teacherGuid, string fullName, 
        [Service] ITeacherService teacherService, [Service] ILogger<ITeacherService> logger)
    {
        var result = await teacherService.CreateTeacherAsync(new TeacherEntity
        {
            TeacherGuid = teacherGuid,
            FullName = fullName,
            Permissions = TeacherPermissions.DefaultAccess
        });

        return result.Match(_ => new TeacherEntity
        {
            FullName = fullName, 
            TeacherGuid = teacherGuid, 
            Permissions = TeacherPermissions.DefaultAccess
        }, 
            exception =>
            {
                logger.LogError(exception, "Error during teacher creation. Teacher: {result}", result);
                throw exception;
            });
    }

    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> GivePermissionsToTeacherAsync(string teacherGuid, IEnumerable<TeacherPermissions> permissions, 
        [Service] ITeacherService teacherService, [Service] ILogger<ITeacherService> logger)
    {
        var teacherPermissions = permissions.Aggregate((prev, next) => prev | next);
        var result = await teacherService.GivePermissionsAsync(teacherGuid, teacherPermissions);
        
        return result.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating teacher's permissions. Teacher guid: {teacherGuid}", teacherGuid);
            throw exception;
        });
    }
}