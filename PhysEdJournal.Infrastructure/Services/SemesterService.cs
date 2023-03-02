using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class SemesterService : ISemesterService
{
    private readonly ApplicationContext _applicationContext;

    public SemesterService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> StartNewSemesterAsync(string semesterName)
    {
        try
        {
            if (!Regex.IsMatch(semesterName, @"\d{4}-\d{4}/\w{5}"))
            {
                return new Result<Unit>(new SemesterNameValidationException());
            }
            
            _applicationContext.Add(new SemesterEntity{Name = semesterName});
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}