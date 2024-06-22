using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands;

public sealed class AddHealthGroupPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required HealthGroupType HealthGroup { get; init; }
}

file sealed class AddHealthGroupValidator : ICommandValidator<AddHealthGroupPayload>
{
    private readonly ApplicationContext _appContext;

    public AddHealthGroupValidator(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        AddHealthGroupPayload commandInput
    )
    {
        var studentCuratorGuid = await _appContext
            .Students.Where(s => s.StudentGuid == commandInput.StudentGuid)
            .Include(s => s.Group)
            .ThenInclude(g => g!.Curator)
            .Select(s => s.Group!.Curator!.TeacherGuid)
            .FirstOrDefaultAsync();

        if (studentCuratorGuid != commandInput.TeacherGuid)
        {
            return new CuratorMismatchError();
        }

        return ValidationResult.Success;
    }
}

public sealed class AddHealthGroupCommand : ICommand<AddHealthGroupPayload, Unit>
{
    private readonly ApplicationContext _appContext;

    public AddHealthGroupCommand(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(AddHealthGroupPayload commandPayload)
    {
        var validation = await new AddHealthGroupValidator(_appContext).ValidateCommandInputAsync(
            commandPayload
        );

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await _appContext.Students.FindAsync(commandPayload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        student.HealthGroup = commandPayload.HealthGroup;

        await _appContext.SaveChangesAsync();

        return Unit.Default;
    }
}
