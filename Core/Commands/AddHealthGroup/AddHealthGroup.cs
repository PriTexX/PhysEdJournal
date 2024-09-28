using DB;
using DB.Tables;
using PResult;

namespace Core.Commands;

public sealed class AddHealthGroupPayload
{
    public required string StudentGuid { get; init; }
    public required string TeacherGuid { get; init; }
    public required HealthGroupType HealthGroup { get; init; }
}

public sealed class AddHealthGroupCommand : ICommand<AddHealthGroupPayload, Unit>
{
    private readonly ApplicationContext _appContext;

    public AddHealthGroupCommand(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(AddHealthGroupPayload payload)
    {
        var student = await _appContext.Students.FindAsync(payload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        student.HealthGroup = payload.HealthGroup;
        student.HealthGroupTeacherId = payload.TeacherGuid;

        await _appContext.SaveChangesAsync();

        return Unit.Default;
    }
}
