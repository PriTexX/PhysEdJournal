using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddPointsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required WorkType WorkType { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddPointsCommandValidator : ICommandValidator<AddPointsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddPointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddPointsCommandPayload commandInput)
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new ActionFromFutureException(commandInput.Date);
        }

        if (commandInput.Points <= 0)
        {
            return new NegativePointAmount();  
        }

        if (commandInput.WorkType == WorkType.ExternalFitness)
        {
            var anotherFitness = await _applicationContext.PointsStudentsHistory
                .AsNoTracking()
                .Where(p => p.StudentGuid == commandInput.StudentGuid && p.WorkType == WorkType.ExternalFitness)
                .FirstOrDefaultAsync();

            if (anotherFitness is not null)
            {
                return new FitnessAlreadyExistsException();
            }
        }

        return ValidationResult.Success;
    }
}

public sealed class AddPointsCommand : ICommand<AddPointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddPointsCommandValidator _validator;

    public AddPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddPointsCommandValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddPointsCommandPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ToResult<Unit>();
        }

        var student = await _applicationContext.Students.FindAsync(commandPayload.StudentGuid);

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
        }
        
        var pointsStudentHistoryEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            Comment = commandPayload.Comment,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            WorkType = commandPayload.WorkType,
            SemesterName = student.CurrentSemesterName,
            TeacherGuid = commandPayload.TeacherGuid
        };

        student.AdditionalPoints += pointsStudentHistoryEntity.Points;
        
        _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        await StudentArchiver.TryArchiveStudentIfHisDebtIsClosed(student, _applicationContext);
        
        return Unit.Default;
    }
}