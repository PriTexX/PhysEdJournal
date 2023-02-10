using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Application.Services;

public interface IStudentService
{
    public Task<Result<StudentPointsHistoryEntity>> AddPointsAsync(string studentGuid, string teacherGuid, int pointsAmount, DateOnly date, WorkType workType, string? comment = null);

    public Task<Result<StudentVisitsHistoryEntity>>
        IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid);

    public Task<Result<ArchivedStudentEntity>> ArchiveStudent(string studentGuid); 

    public Task<Result<Unit>> CreateStudentAsync(StudentEntity studentEntity);
    
    public Task<Result<StudentEntity?>> GetStudentAsync(string guid);

    public Task<Result<Unit>> UpdateStudentAsync(string guid, StudentEntity updatedStudent);

    public Task<Result<Unit>> DeleteStudentAsync(string guid);
}