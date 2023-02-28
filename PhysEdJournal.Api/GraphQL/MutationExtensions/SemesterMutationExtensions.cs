using PhysEdJournal.Api.GraphQL.ScalarTypes;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;

namespace PhysEdJournal.Api.GraphQL.MutationExtensions;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SemesterMutationExtensions
{
    [Error(typeof(SemesterNameValidationException))]
    public async Task<Success> StartNewSemester(string semesterName, [Service] ISemesterService semesterService)
    {
        var res = await semesterService.StartNewSemesterAsync(semesterName);
        throw new SemesterNameValidationException();
        return res.Match(_ => true, exception => throw exception);
    }
}