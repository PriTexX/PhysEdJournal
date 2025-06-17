using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.Extensions.Caching.Memory;

namespace Api;

public sealed class NotEnoughPermissionsError()
    : Exception("Teacher does not have enough permissions to do this") { }

public sealed class PermissionValidator
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
                return new TeacherNotFoundError();
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

        if (!HasEnoughPermissions(teacher.Permissions, requiredPermissions))
        {
            return new NotEnoughPermissionsError();
        }

        return true;
    }

    private static bool HasEnoughPermissions(
        TeacherPermissions permissions,
        TeacherPermissions requiredPermissions
    )
    {
        if (permissions.HasFlag(TeacherPermissions.Disabled))
        {
            return false;
        }

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
