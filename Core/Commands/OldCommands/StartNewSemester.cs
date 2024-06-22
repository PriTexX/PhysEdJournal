using DB;
using DB.Tables;
using Microsoft.EntityFrameworkCore;
using PResult;

namespace Core.Commands.OldCommands;

public sealed class StartNewSemesterCommand : ICommand<string, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public StartNewSemesterCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(string semesterName)
    {
        var isDuplicate = await _applicationContext.Semesters.AnyAsync(s => s.Name == semesterName);

        if (isDuplicate)
        {
            return new Exception("Exists");
        }

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
