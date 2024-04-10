using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class DeActivateStudentCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public DeActivateStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(string studentGuid)
    {
        var rowsAffected = await _applicationContext
            .Students.Where(s => s.StudentGuid == studentGuid)
            .ExecuteUpdateAsync(p => p.SetProperty(s => s.IsActive, false));

        if (rowsAffected == 0)
        {
            return new StudentNotFoundException(studentGuid);
        }

        return Unit.Default;
    }
}
