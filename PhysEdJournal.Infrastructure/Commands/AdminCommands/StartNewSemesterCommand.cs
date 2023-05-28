using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class StartNewSemesterCommandPayload
{
    public required string SemesterName { get; init; }
}

internal sealed class StartNewSemesterCommandValidator : ICommandValidator<StartNewSemesterCommandPayload>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(StartNewSemesterCommandPayload commandInput)
    {
        if (!Regex.IsMatch(commandInput.SemesterName, @"\d{4}-\d{4}/\w{5}"))
        {
            return ValueTask.FromResult<ValidationResult>(new SemesterNameValidationException());
        }

        return ValueTask.FromResult(ValidationResult.Success);
    }
}

public sealed class StartNewSemesterCommand : ICommand<StartNewSemesterCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly IMemoryCache _memoryCache;
    private readonly StartNewSemesterCommandValidator _validator;

    public StartNewSemesterCommand(ApplicationContext applicationContext, IMemoryCache memoryCache)
    {
        _applicationContext = applicationContext;
        _memoryCache = memoryCache;
        _validator = new StartNewSemesterCommandValidator();
    }
    
    public async Task<Result<Unit>> ExecuteAsync(StartNewSemesterCommandPayload commandPayload)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var currentSemester = await _applicationContext.Semesters.Where(s => s.IsCurrent == true).SingleOrDefaultAsync();
        if (currentSemester is not null)
        {
            currentSemester.IsCurrent = false;
            _applicationContext.Update(currentSemester);
        }

        var semester = new SemesterEntity { Name = commandPayload.SemesterName, IsCurrent = true };
            
        _applicationContext.Add(semester);
        await _applicationContext.SaveChangesAsync();
            
        using var entry = _memoryCache.CreateEntry("activeSemester");
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        entry.Value = semester;

        return Unit.Default;
    }
}