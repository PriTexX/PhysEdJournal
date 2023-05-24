using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Exceptions.GroupExceptions;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class AssignCuratorCommandPayload
{
    public required string GroupName { get; init; }
    public required string TeacherGuid { get; init; }
}

public sealed class AssignCuratorCommand : ICommand<AssignCuratorCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public AssignCuratorCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    public async Task<Result<Unit>> ExecuteAsync(AssignCuratorCommandPayload commandPayload)
    {
        var teacher = await _applicationContext.Teachers.FindAsync(commandPayload.TeacherGuid);
        
        if (teacher == null)
        {
            return new Result<Unit>(new TeacherNotFoundException(commandPayload.TeacherGuid));
        }
    
        var group = await _applicationContext.Groups.FindAsync(commandPayload.GroupName);
    
        if (group == null)
        {
            return new Result<Unit>(new GroupNotFoundException(commandPayload.GroupName));
        }

        group.Curator = teacher;
        group.CuratorGuid = teacher.TeacherGuid;

        _applicationContext.Groups.Update(group);
        await _applicationContext.SaveChangesAsync();

        return new Result<Unit>(Unit.Default);
    }
}