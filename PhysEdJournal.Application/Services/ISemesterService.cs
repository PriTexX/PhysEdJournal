using LanguageExt;
using LanguageExt.Common;

namespace PhysEdJournal.Application.Services;

public interface ISemesterService
{
    public Task<Result<Unit>> StartNewSemesterAsync(string semesterName);
}