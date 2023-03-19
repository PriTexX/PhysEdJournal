using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Application.Services;

public interface ITeacherService
{
    public Task<Result<TeacherEntity>> GivePermissionsAsync(string callerGuid,string teacherGuid, TeacherPermissions type);

    public Task<Result<Unit>> CreateTeacherAsync(string callerGuid, TeacherEntity teacherEntity);
}