using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class SemesterService : ISemesterService
{
    private readonly ApplicationContext _applicationContext;
    private readonly ILogger<SemesterService> _logger;

    public SemesterService(ApplicationContext applicationContext, ILogger<SemesterService> logger)
    {
        _logger = logger;
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> StartNewSemesterAsync(string semesterName)
    {
        try
        {
            _applicationContext.Add(new SemesterEntity{Name = semesterName});
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            _logger.LogError(err.ToString());
            return new Result<Unit>(err);
        }
    }
}