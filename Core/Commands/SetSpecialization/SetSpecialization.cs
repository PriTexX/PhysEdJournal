using DB;
using DB.Tables;
using PResult;

namespace Core.Commands;

public sealed class SetSpecializationPayload
{
    public required string StudentGuid { get; set; }
    public required SpecializationType Specialization { get; set; }
}

public sealed class SetSpecializationCommand : ICommand<SetSpecializationPayload, Unit>
{
    private readonly ApplicationContext _appCtx;

    public SetSpecializationCommand(ApplicationContext appCtx)
    {
        _appCtx = appCtx;
    }

    public async Task<Result<Unit>> ExecuteAsync(SetSpecializationPayload payload)
    {
        var student = await _appCtx.Students.FindAsync(payload.StudentGuid);

        if (student is null)
        {
            return new StudentNotFoundError();
        }

        student.Specialization = payload.Specialization;

        await _appCtx.SaveChangesAsync();

        return Unit.Default;
    }
}
