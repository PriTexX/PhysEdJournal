﻿using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PhysEdJournal.Application.Services;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Validators.Permissions;
using static PhysEdJournal.Core.Permissions.Constants;

namespace PhysEdJournal.Infrastructure.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly ApplicationContext _applicationContext;
    private readonly PermissionValidator _permissionValidator;
    private readonly IMemoryCache _memoryCache;

    public TeacherService(ApplicationContext applicationContext, PermissionValidator permissionValidator)
    {
        _applicationContext = applicationContext;
        _permissionValidator = permissionValidator;
    }
    
    public async Task<Result<TeacherEntity>> GivePermissionsAsync(string callerGuid, string teacherGuid, TeacherPermissions type) // TODO может стоит создать роль суперадмина для таких действий
    {
        try
        {
            // if (callerGuid == teacherGuid)
            // {
            //     throw new ArgumentException("Cannot update your own permissions");
            // }
            
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

            if (teacher is null)
            {
                return await Task.FromResult(new Result<TeacherEntity>(new TeacherNotFoundException(teacherGuid)));
            }

            teacher.Permissions = type;
            
            using var entry = _memoryCache.CreateEntry(teacherGuid);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.Value = teacher;
            
            _applicationContext.Teachers.Update(teacher);
            await _applicationContext.SaveChangesAsync();

            return teacher;
        }
        catch (Exception e)
        {
            return new Result<TeacherEntity>(e);
        }
    }

    public async Task<Result<Unit>> CreateTeacherAsync(string callerGuid, TeacherEntity teacherEntity)
    {
        try
        {
            await _permissionValidator.ValidateTeacherPermissionsAndThrow(callerGuid, FOR_ONLY_ADMIN_USE_PERMISSIONS);
            
            var teacherGuid = await _applicationContext.Teachers
                .AsNoTracking()
                .Where(t => t.TeacherGuid == teacherEntity.TeacherGuid)
                .Select(t => t.TeacherGuid)
                .FirstOrDefaultAsync();

            if (teacherGuid is not null)
                return new Result<Unit>(new TeacherAlreadyExistsException(teacherGuid));
            
            await _applicationContext.Teachers.AddAsync(teacherEntity);
            await _applicationContext.SaveChangesAsync();
        
            return Unit.Default;
        }
        catch (Exception err)
        {
            return new Result<Unit>(err);
        }
    }
}