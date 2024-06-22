using DB;
using DB.Tables;
using Microsoft.Extensions.Caching.Memory;
using PResult;

namespace Core.Commands.OldCommands;

public sealed class GivePermissionsCommandPayload
{
    public required string TeacherGuid { get; init; }
    public required TeacherPermissions TeacherPermissions { get; init; }
}

internal sealed class GivePermissionsCommandValidator
    : ICommandValidator<GivePermissionsCommandPayload>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(
        GivePermissionsCommandPayload commandInput
    )
    {
        if (commandInput.TeacherPermissions == TeacherPermissions.SuperUser)
        {
            return ValidationResult.Create(new Exception("Cannot grant SuperUser"));
        }

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

    public async Task<Result<TeacherEntity>> ExecuteAsync(
        GivePermissionsCommandPayload commandPayload
    )
    {
        var validationResult = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validationResult.IsFailed)
        {
            return validationResult.ValidationException;
        }

        var teacher = await _applicationContext.Teachers.FindAsync(commandPayload.TeacherGuid);

        if (teacher is null)
        {
            return new TeacherNotFoundError();
        }

        teacher.Permissions = commandPayload.TeacherPermissions;

        using var entry = _memoryCache.CreateEntry(commandPayload.TeacherGuid);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        entry.Value = teacher;

        _applicationContext.Teachers.Update(teacher);
        await _applicationContext.SaveChangesAsync();

        return teacher;
    }
}
