﻿using Core.Commands;
using DB;
using DB.Tables;
using Microsoft.Extensions.Caching.Memory;

namespace GraphQL.Api;

public sealed class GraphQLNotEnoughPermissionsError()
    : Exception("Teacher does not have enough permissions to do this") { }

public sealed class GraphQLPermissionValidator
{
    private readonly ApplicationContext _applicationContext;
    private readonly IMemoryCache _memoryCache;

    public GraphQLPermissionValidator(
        ApplicationContext applicationContext,
        IMemoryCache memoryCache
    )
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

        var hasEnough = HasEnoughPermissions(teacher.Permissions, requiredPermissions);

        if (!hasEnough)
        {
            return new GraphQLNotEnoughPermissionsError();
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
