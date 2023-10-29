using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api;

public class PermissionValidator
{
    private readonly ApplicationContext _applicationContext;
    private readonly IMemoryCache _memoryCache;

    public PermissionValidator(ApplicationContext applicationContext, IMemoryCache memoryCache)
    {
        _applicationContext = applicationContext;
        _memoryCache = memoryCache;
    }

    public async ValueTask<LanguageExt.Common.Result<bool>> ValidateTeacherPermissions(
        string teacherGuid,
        TeacherPermissions requiredPermissions
    )
    {
        _memoryCache.TryGetValue(teacherGuid, out TeacherEntity? teacher);

        if (teacher is null)
        {
            teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

            if (teacher is null)
            {
                return new LanguageExt.Common.Result<bool>(
                    new TeacherNotFoundException(teacherGuid)
                );
            }

            _memoryCache.Set(
                teacherGuid,
                teacher,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                }
            );
        }

        if (requiredPermissions == TeacherPermissions.DefaultAccess)
        {
            return new LanguageExt.Common.Result<bool>(true);
        }

        var hasEnough = HasEnoughPermissions(teacher.Permissions, requiredPermissions);

        if (!hasEnough)
        {
            return new LanguageExt.Common.Result<bool>(
                new NotEnoughPermissionsException(
                    teacherGuid,
                    teacher.Permissions,
                    requiredPermissions
                )
            );
        }

        return hasEnough;
    }

    public async ValueTask ValidateTeacherPermissionsAndThrow(
        string teacherGuid,
        TeacherPermissions requiredPermissions
    )
    {
        var validationResult = await ValidateTeacherPermissions(teacherGuid, requiredPermissions);

        validationResult.Match(_ => true, exception => throw exception);
    }

    private static bool HasEnoughPermissions(
        TeacherPermissions permissions,
        TeacherPermissions requiredPermissions
    )
    {
        if (permissions.HasFlag(TeacherPermissions.SuperUser))
        {
            return true;
        }

        if (requiredPermissions == 0)
        {
            return true;
        }

        return (permissions & requiredPermissions) != 0;
    }
}
