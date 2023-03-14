using System.Security.Claims;
using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SemesterMutationExtensions
{
    [Error(typeof(SemesterNameValidationException))]
    [Error(typeof(NotEnoughPermissionsException))]
    [Error(typeof(TeacherNotFoundException))]
    public async Task<Success> StartNewSemester(string semesterName, [Service] ISemesterService semesterService, [Service] ILogger<ISemesterService> logger, ClaimsPrincipal claimsPrincipal)
    {
        var teacherGuid = claimsPrincipal.FindFirstValue("IndividualGuid");

        var res = await semesterService.StartNewSemesterAsync(teacherGuid, semesterName);
        
        return res.Match(_ => true, exception =>
        {
            logger.LogError(exception, "Error during starting a new semester");
            throw exception;
        });
    }
}