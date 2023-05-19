using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class TeacherMutationExtensions
{
    
    [Error(typeof(TeacherAlreadyExistsException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<TeacherEntity> CreateTeacherAsync(string teacherGuid, string fullName, 
        [Service] TeacherService teacherService, [Service] ILogger<TeacherService> logger, ClaimsPrincipal claimsPrincipal
        )
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
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
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(CannotGrantSuperUserPermissionsException))]
    public async Task<Success> GivePermissionsToTeacherAsync(string teacherGuid, IEnumerable<TeacherPermissions> permissions, 
        [Service] TeacherService teacherService, [Service] ILogger<TeacherService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        
        var teacherPermissions = permissions.Aggregate((prev, next) => prev | next);
        var result = await teacherService.GivePermissionsAsync(teacherGuid, teacherPermissions);
        
        return result.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during updating teacher's permissions. Teacher guid: {teacherGuid}", teacherGuid);
            throw exception;
        });
    }

    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    public async Task<Success> CreateCompetition(string competitionName, [Service] TeacherService teacherService, [Service] ILogger<TeacherService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var result = await teacherService.CreateCompetitionAsync(competitionName);

        return result.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during creating competition with name: {competitionName}", competitionName);
            throw exception;
        });
    }
    
    [Error(typeof(TeacherNotFoundException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(CompetitionNotFoundException))]
    public async Task<Success> DeleteCompetition(string competitionName, [Service] TeacherService teacherService, [Service] ILogger<TeacherService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var result = await teacherService.DeleteCompetitionAsync(competitionName);

        return result.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during deleting competitions with name: {competitionName}", competitionName);
            throw exception;
        });
    }
}