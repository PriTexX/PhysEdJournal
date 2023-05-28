using LanguageExt.Common;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class GivePermissionsCommandPayload
{
    public required string TeacherGuid { get; init; }
    public required TeacherPermissions Type { get; init; }
}

internal sealed class GivePermissionsCommandValidator : ICommandValidator<GivePermissionsCommandPayload>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(GivePermissionsCommandPayload commandInput)
    {
        if (commandInput.Type == TeacherPermissions.SuperUser)
            return ValidationResult.Create(new CannotGrantSuperUserPermissionsException(commandInput.TeacherGuid));

        return ValidationResult.Success;
    }
}

public sealed class GivePermissionsCommand : ICommand<GivePermissionsCommandPayload, TeacherEntity>
{
    private readonly ApplicationContext _applicationContext;
    private readonly GivePermissionsCommandValidator _validator;
    private readonly IMemoryCache _memoryCache;

    public GivePermissionsCommand(ApplicationContext applicationContext, IMemoryCache memoryCache)
    {
        _applicationContext = applicationContext;
        _memoryCache = memoryCache;
        _validator = new GivePermissionsCommandValidator();
    }
    
    public async Task<Result<TeacherEntity>> ExecuteAsync(GivePermissionsCommandPayload commandPayload)
    {
        var validationResult = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validationResult.IsFailed)
        {
            return validationResult.ToResult<TeacherEntity>();
        }

        var teacher = await _applicationContext.Teachers.FindAsync(commandPayload.TeacherGuid);

        if (teacher is null)
        {
            return new Result<TeacherEntity>(new TeacherNotFoundException(commandPayload.TeacherGuid));
        }

        teacher.Permissions = commandPayload.Type;

        using var entry = _memoryCache.CreateEntry(commandPayload.TeacherGuid);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        entry.Value = teacher;

        _applicationContext.Teachers.Update(teacher);
        await _applicationContext.SaveChangesAsync();

        return teacher;
    }
}