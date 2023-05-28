using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddPointsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string SemesterName { get; init; }
    public required string TeacherGuid { get; init; }
    public required WorkType WorkType { get; init; }
    public string? Comment { get; init; }
}

internal sealed class AddPointsCommandValidator : ICommandValidator<AddPointsCommandPayload>
{
    public ValueTask<ValidationResult> ValidateCommandInputAsync(AddPointsCommandPayload commandInput)
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return ValidationResult.Create(new ActionFromFutureException(commandInput.Date));
        }

        if (commandInput.Points <= 0)
        {
            return ValidationResult.Create(new NegativePointAmount());  
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
        _validator = new AddPointsCommandValidator();
    }

    public async Task<Result<Unit>> ExecuteAsync(AddPointsCommandPayload commandPayload)
    {
        var validation = await _validator.ValidateCommandInputAsync(commandPayload);

        if (validation.IsFailed)
        {
            return validation.ToResult<Unit>();
        }
        
        var pointsStudentHistoryEntity = new PointsStudentHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            Comment = commandPayload.Comment,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            WorkType = commandPayload.WorkType,
            SemesterName = commandPayload.SemesterName,
            TeacherGuid = commandPayload.TeacherGuid
        };
        
        var student = await _applicationContext.Students.FindAsync(pointsStudentHistoryEntity.StudentGuid);

        if (student is null)
        {
            return new Result<Unit>(new StudentNotFoundException(pointsStudentHistoryEntity.StudentGuid));
        }

        student.AdditionalPoints += pointsStudentHistoryEntity.Points;

        pointsStudentHistoryEntity.SemesterName = student.CurrentSemesterName;
        _applicationContext.PointsStudentsHistory.Add(pointsStudentHistoryEntity);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        return Unit.Default;
    }
}