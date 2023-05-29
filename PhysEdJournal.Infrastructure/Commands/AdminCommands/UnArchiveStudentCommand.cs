using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands.AdminCommands;

public sealed class UnArchiveStudentCommandPayload
{
    public required string StudentGuid { get; init; }
    public required string SemesterName { get; init; }
}

public sealed class UnArchiveStudentCommand : ICommand<UnArchiveStudentCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;

    public UnArchiveStudentCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async Task<Result<Unit>> ExecuteAsync(UnArchiveStudentCommandPayload commandPayload)
    {
        var student = await _applicationContext.Students.FindAsync(commandPayload.StudentGuid);
            if (student is null)
            {
                return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
            }
        
            var archivedStudent = await _applicationContext.ArchivedStudents.FindAsync(commandPayload.StudentGuid, commandPayload.SemesterName);
            if (archivedStudent is null)
            {
                return new Result<Unit>(new ArchivedStudentNotFound(commandPayload.StudentGuid, commandPayload.SemesterName));
            }

            await using var transaction = await _applicationContext.Database.BeginTransactionAsync();
            
            var pointsStudentHistory = await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == commandPayload.StudentGuid && h.SemesterName == commandPayload.SemesterName)
                .ToListAsync();
    
            student.AdditionalPoints = pointsStudentHistory.Aggregate(0, (prev, next) => prev + next.Points);
            
            var standardsStudentHistory = await _applicationContext.StandardsStudentsHistory
                .Where(h => h.StudentGuid == commandPayload.StudentGuid && h.SemesterName == commandPayload.SemesterName)
                .ToListAsync();
            
            student.PointsForStandards = standardsStudentHistory.Aggregate(0, (prev, next) => prev + next.Points);
    
            student.HasDebtFromPreviousSemester = false;
            student.ArchivedVisitValue = 0;
            student.Visits = archivedStudent.Visits;

            _applicationContext.Students.Update(student);
            await _applicationContext.SaveChangesAsync();

            await _applicationContext.VisitsStudentsHistory
                .Where(h => h.StudentGuid == commandPayload.StudentGuid && h.IsArchived == true)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.IsArchived, false));

            await _applicationContext.PointsStudentsHistory
                .Where(h => h.StudentGuid == commandPayload.StudentGuid && h.SemesterName == commandPayload.SemesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.SemesterName, student.CurrentSemesterName)
                    .SetProperty(s => s.IsArchived, false));
            
            await _applicationContext.StandardsStudentsHistory
                .Where(h => h.StudentGuid == commandPayload.StudentGuid && h.SemesterName == commandPayload.SemesterName)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.SemesterName, student.CurrentSemesterName)
                    .SetProperty(s => s.IsArchived, false));

            await transaction.CommitAsync();
            return Unit.Default;
    }
}