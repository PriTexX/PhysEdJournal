using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class SemesterService : ISemesterService
{
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;

    public SemesterService(ApplicationContext applicationContext, PermissionValidator permissionValidator)
    {
        _applicationContext = applicationContext;
        _permissionValidator = permissionValidator;
    }

    public async Task<Result<Unit>> StartNewSemesterAsync(string teacherGuid, string semesterName)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(teacherGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            if (!Regex.IsMatch(semesterName, @"\d{4}-\d{4}/\w{5}"))
            {
                return new Result<Unit>(new SemesterNameValidationException());
            }

            var currentSemester = await _applicationContext.Semesters.Where(s => s.IsCurrent == true).SingleOrDefaultAsync();
            if (currentSemester is not null)
            {
                currentSemester.IsCurrent = false;
                _applicationContext.Update(currentSemester);
            }

            _applicationContext.Add(new SemesterEntity{Name = semesterName, IsCurrent = true});
            await _applicationContext.SaveChangesAsync();

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}