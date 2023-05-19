using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Exceptions.SemesterExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using static PhysEdJournal.Core.Constants.PermissionConstants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class SemesterService
{
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;
    private readonly IMemoryCache _memoryCache;

    public SemesterService(ApplicationContext applicationContext, PermissionValidator permissionValidator, IMemoryCache memoryCache)
    {
        _applicationContext = applicationContext;
        _permissionValidator = permissionValidator;
        _memoryCache = memoryCache;
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

            var semester = new SemesterEntity { Name = semesterName, IsCurrent = true };
            
            _applicationContext.Add(semester);
            await _applicationContext.SaveChangesAsync();
            
            using var entry = _memoryCache.CreateEntry("activeSemester");
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.Value = semester;

            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}