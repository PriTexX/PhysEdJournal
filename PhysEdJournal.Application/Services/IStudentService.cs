using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;

namespace PhysEdJournal.Application.Services;

public interface IStudentService
{
    public Task<Result<Unit>> AddPointsAsync(PointsStudentHistoryEntity pointsStudentHistoryEntity);
    
    public Task<Result<Unit>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid);

    public Task<Result<ArchivedStudentEntity>> ArchiveStudentAsync(string studentGuid, string currentSemesterName, bool isForceMode = false);

    public Task<Result<Unit>> UpdateStudentsInfoAsync();
    
    public Task<Result<Unit>> CreateStudentAsync(StudentEntity studentEntity);
    
    public Task<Result<StudentEntity?>> GetStudentAsync(string guid);

    public Task<Result<Unit>> UpdateStudentAsync(StudentEntity updatedStudent);

    public Task<Result<Unit>> DeleteStudentAsync(string guid);
}
