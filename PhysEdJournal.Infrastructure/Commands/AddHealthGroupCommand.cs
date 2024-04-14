using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddHealthGroupPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required HealthGroupType HealthGroup { get; init; }
}

file sealed class AddHealthGroupCommandValidator : ICommandValidator<AddHealthGroupPayload>
{
    private readonly ApplicationContext _appContext;

    public AddHealthGroupCommandValidator(ApplicationContext appContext)
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
            return new CuratorGuidMismatch();
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
        var validation = await new AddHealthGroupCommandValidator(
            _appContext
        ).ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await _appContext.Students.FindAsync(commandPayload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundException(commandPayload.StudentGuid);
        }

        student.HealthGroup = commandPayload.HealthGroup;

        await _appContext.SaveChangesAsync();

        return Unit.Default;
    }
}
