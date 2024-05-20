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

    public async ValueTask<PResult.Result<bool>> ValidateTeacherPermissions(
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
                return new TeacherNotFoundException(teacherGuid);
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

        var hasEnough = HasEnoughPermissions(teacher.Permissions, requiredPermissions);

        if (!hasEnough)
        {
            return new NotEnoughPermissionsException(
                teacherGuid,
                teacher.Permissions,
                requiredPermissions
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
        if (requiredPermissions == TeacherPermissions.DefaultAccess)
        {
            return true;
        }

        if (permissions.HasFlag(TeacherPermissions.SuperUser))
        {
            return true;
        }

        if (requiredPermissions.HasFlag(TeacherPermissions.SuperUser))
        {
            return false;
        }

        if (permissions.HasFlag(TeacherPermissions.AdminAccess))
        {
            return true;
        }

        return (permissions & requiredPermissions) != 0;
    }
}
