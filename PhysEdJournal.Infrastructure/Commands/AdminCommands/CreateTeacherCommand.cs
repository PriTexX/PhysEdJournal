using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class CreateTeacherCommandPayload
{
    public required string TeacherGuid { get; init; }
    public required string FullName { get; init; }
    public required TeacherPermissions Permissions { get; init; }
}

public sealed class CreateTeacherCommand : ICommand<CreateTeacherCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public CreateTeacherCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(CreateTeacherCommandPayload commandPayload)
    {
        var teacherGuid = await _applicationContext.Teachers
            .AsNoTracking()
            .Where(t => t.TeacherGuid == commandPayload.TeacherGuid)
            .Select(t => t.TeacherGuid)
            .FirstOrDefaultAsync();

        if (teacherGuid is not null)
        {
            return new TeacherAlreadyExistsException(teacherGuid);
        }

        var teacherEntity = new TeacherEntity
        {
            TeacherGuid = commandPayload.TeacherGuid,
            FullName = commandPayload.FullName,
            Permissions = commandPayload.Permissions,
        };

        await _applicationContext.Teachers.AddAsync(teacherEntity);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
