using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class DeletePointsCommandPayload
{
    public required string StudentGuid { get; init; }
    
    public required int HistoryId { get; init; }
    
    public required string TeacherGuid { get; init; }

    public required bool IgnoreDateIntervalCheck { get; init; } = false;
}

internal sealed class DeletePointsCommandValidator : ICommandValidator<DeletePointsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public DeletePointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public ValueTask<ValidationResult> ValidateCommandInputAsync(DeletePointsCommandPayload commandInput)
    {
        return ValidationResult.Success;
    }
}

public sealed class DeletePointsCommand : ICommand<DeletePointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly DeletePointsCommandValidator _validator;

    public DeletePointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new DeletePointsCommandValidator(applicationContext);
    }
    
    public async Task<Result<Unit>> ExecuteAsync(DeletePointsCommandPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);
        
        if (validation.IsFailed)
        {
            return validation.ToResult<Unit>();
        }

        var student = await _applicationContext.Students.FirstOrDefaultAsync(s => s.StudentGuid == commandPayload.StudentGuid);

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(commandPayload.StudentGuid));
        }

        var history = await _applicationContext.PointsStudentsHistory.FirstOrDefaultAsync(s => s.Id == commandPayload.HistoryId);

        if (history is null)
        { // TODO: сделать нормальную ошибку
            return new Result<Unit>(new Exception($"No history with id = {commandPayload.HistoryId} in student with guid {commandPayload.StudentGuid}"));
        }
        
        if (history.Date.DayNumber - DateOnly.FromDateTime(DateTime.Now).DayNumber > 7 && !commandPayload.IgnoreDateIntervalCheck) //TODO: убрать хардкод
        { // TODO: сделать нормальную ошибку
            return new Result<Unit>(new Exception($"You cannot delete points if they existed more than {7} days ago"));
        }
        
        student.AdditionalPoints -= history.Points;
        _applicationContext.PointsStudentsHistory.Remove(history);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        return new Result<Unit>();
    }
}