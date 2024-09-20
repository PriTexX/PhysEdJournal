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

file sealed class AddHealthGroupValidator(ApplicationContext appContext) : ICommandValidator<AddHealthGroupPayload>
{
    public async ValueTask<ValidationResult> ValidateCommandInputAsync(
        AddHealthGroupPayload commandInput
    )
    {
        var teacher = await appContext.Teachers.FindAsync(commandInput.TeacherGuid);
        
        return teacher == null ? new CuratorMismatchError() : ValidationResult.Success;
    }
}

public sealed class AddHealthGroupCommand(ApplicationContext appContext) : ICommand<AddHealthGroupPayload, Unit>
{
    public async Task<Result<Unit>> ExecuteAsync(AddHealthGroupPayload commandPayload)
    {
        var validation = await new AddHealthGroupValidator(appContext).ValidateCommandInputAsync(
            commandPayload
        );

        if (validation.IsFailed)
        {
            return validation.ValidationException;
        }

        var student = await appContext.Students.FindAsync(commandPayload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        student.HealthGroup = commandPayload.HealthGroup;
        student.HealthGroupProvider = await appContext.Teachers.FindAsync(commandPayload.TeacherGuid);

        await appContext.SaveChangesAsync();

        return Unit.Default;
    }
}
