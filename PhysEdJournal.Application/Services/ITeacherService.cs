using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Application.Services;

public interface ITeacherService
{
    public Task<Result<TeacherEntity>> GivePermissions(string teacherGuid, TeacherPermissions type);

    public Task<Result<Unit>> CreateTeacherAsync(TeacherEntity teacherEntity);
    
    public Task<Result<TeacherEntity?>> GetTeacherAsync(string guid);
    
    public Task<Result<Unit>> UpdateTeacherAsync(TeacherEntity updatedTeacher);
    
    public Task<Result<Unit>> DeleteTeacherAsync(string guid);
}