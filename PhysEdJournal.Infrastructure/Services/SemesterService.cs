using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class SemesterService : ISemesterService
{
    private readonly ApplicationContext _applicationContext;
    private readonly TxtFileConfig _fileConfig;

    public SemesterService(ApplicationContext applicationContext, TxtFileConfig fileConfig)
    {
        _applicationContext = applicationContext;
        _fileConfig = fileConfig;
    }

    public async Task<Result<Unit>> StartNewSemesterAsync(string semesterName)
    {
        try
        {
            await _applicationContext.AddAsync(SemesterEntity.FromString(semesterName));
            await _applicationContext.SaveChangesAsync();
            
            _fileConfig.WriteTextToFile(semesterName);
            
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}