using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Api.Permissions;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using static PhysEdJournal.Api.Permissions.PermissionsConstants;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SemesterMutationExtensions
{
    [Error(typeof(SemesterNameValidationException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> StartNewSemester(string semesterName, [Service] ISemesterService semesterService, [Service] ILogger<ISemesterService> logger,
        [Service] PermissionValidator permissionValidator, ClaimsPrincipal claimsPrincipal)
    {
        var callerGuid = claimsPrincipal.FindFirstValue("IndividualGuid");
        var validationResult = await permissionValidator.ValidateTeacherPermissions(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
        validationResult.Match(_ => true, exception => throw exception);
        
        
        var res = await semesterService.StartNewSemesterAsync(semesterName);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during starting a new semester");
            throw exception;
        });
    }
}