using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Application.Services;

public interface IStudentService
{
    public Task<Result<Unit>> AddPointsAsync(PointsStudentHistoryEntity pointsStudentHistoryEntity);
    
    public Task<Result<Unit>> IncreaseVisitsAsync(string studentGuid, DateOnly date, string teacherGuid);

    public Task<Result<Unit>> AddPointsForStandardsAsync(StandardStudentHistoryEntity standardStudentHistoryEntity);

    public Task<Result<ArchivedStudentEntity>> ArchiveStudentAsync(string teacherGuid, string studentGuid, bool isForceMode = false);

    public Task<Result<Unit>> UnArchiveStudentAsync(string teacherGuid, string studentGuid, string semesterName);

    public Task<Result<Unit>> UpdateStudentsInfoAsync(string teacherGuid);

    public Task<Result<Unit>> DeActivateStudentAsync(string teacherGuid, string studentGuid);
    
    public Task<Result<Unit>> ActivateStudentAsync(string teacherGuid, string studentGuid);
}
