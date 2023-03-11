using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.TeacherExceptions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Api.Permissions;

public class PermissionValidator
{
    private readonly ApplicationContext _applicationContext;

    public PermissionValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<LanguageExt.Common.Result<bool>> ValidateTeacherPermissions(string teacherGuid, TeacherPermissions requiredPermissions)
    {
        var teacher = await _applicationContext.Teachers.FindAsync(teacherGuid);

        if (teacher is null)
        {
            return new LanguageExt.Common.Result<bool>(new TeacherNotFoundException(teacherGuid));
        }
        
        var hasEnough = HasEnoughPermissions(teacher.Permissions, requiredPermissions);

        if (!hasEnough)
            return new LanguageExt.Common.Result<bool>(new NotEnoughPermissionsException(teacherGuid, teacher.Permissions, requiredPermissions));

        return hasEnough;
    }

    private static bool HasEnoughPermissions(TeacherPermissions permissions, TeacherPermissions requiredPermissions)
    {
        if (permissions.HasFlag(TeacherPermissions.AdminAccess))
            return true;

        if (requiredPermissions == 0)
            return true;

        return (permissions & requiredPermissions) != 0;
    }
}