using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Application.Services;

public interface IStudentService
{
    public Task<Result<PointsStudentHistoryEntity>> AddPointsAsync(string studentGuid, string teacherGuid, int pointsAmount, DateOnly date, WorkType workType, string? comment = null);
    
    public Task<Result<VisitsStudentHistoryEntity>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid);

    public Task<Result<ArchivedStudentEntity>> ArchiveStudent(string studentGuid, bool isForceMode = false);

    public Task<Result<Unit>> UpdateStudentInfoAsync();
    
    public Task<Result<Unit>> CreateStudentAsync(StudentEntity studentEntity);
    
    public Task<Result<StudentEntity?>> GetStudentAsync(string guid);

    public Task<Result<Unit>> UpdateStudentAsync(StudentEntity updatedStudent);

    public Task<Result<Unit>> DeleteStudentAsync(string guid);
}
