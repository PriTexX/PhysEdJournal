using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PResult;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class StartNewSemesterCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public StartNewSemesterCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(string semesterName)
    {
        var currentSemester = await _applicationContext
            .Semesters.Where(s => s.IsCurrent == true)
            .SingleOrDefaultAsync();

        if (currentSemester is not null)
        {
            if (currentSemester.Name == semesterName)
            {
                return Unit.Default;
            }

            currentSemester.IsCurrent = false;
            _applicationContext.Update(currentSemester);
        }

        var semester = new SemesterEntity { Name = semesterName, IsCurrent = true };

        _applicationContext.Add(semester);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}
