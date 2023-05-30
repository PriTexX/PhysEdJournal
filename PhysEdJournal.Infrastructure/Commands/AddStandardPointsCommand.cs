using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Core.Entities.DB;
using PhysEdJournal.Core.Entities.Types;
using PhysEdJournal.Core.Exceptions.DateExceptions;
using PhysEdJournal.Core.Exceptions.StandardExceptions;
using PhysEdJournal.Core.Exceptions.StudentExceptions;
using PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;
using PhysEdJournal.Infrastructure.Database;
using PhysEdJournal.Infrastructure.Services;
using static PhysEdJournal.Core.Constants.PointsConstants;

namespace PhysEdJournal.Infrastructure.Commands;

public sealed class AddStandardPointsCommandPayload
{
    public required string StudentGuid { get; init; }
    public required DateOnly Date { get; init; }
    public required int Points { get; init; }
    public required string TeacherGuid { get; init; }
    public required StandardType StandardType { get; init; }
}

internal sealed class AddStandardPointsCommandValidator : ICommandValidator<AddStandardPointsCommandPayload>
{
    private readonly ApplicationContext _applicationContext;

    public AddStandardPointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddStandardPointsCommandPayload commandInput)
    {
        if (commandInput.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            return new ActionFromFutureException(commandInput.Date);
        }
        
        if (commandInput.Points == MAX_POINTS_FOR_STANDARDS)
        {
            return new OverAbundanceOfPointsForStudentException(commandInput.StudentGuid);
        }

        if (commandInput.Points % 2 != 0)
        {
            return new NonRegularPointsValueException(commandInput.Points);
        }

        if (commandInput.Points <= 0)
        {
            return new NegativePointAmount();
        }
        
        var hasDuplicateRow = await _applicationContext.StandardsStudentsHistory
            .AnyAsync(s => s.StudentGuid == commandInput.StudentGuid && s.StandardType == commandInput.StandardType);
        if (hasDuplicateRow)
        {
            return new StandardAlreadyExistsException(commandInput.StudentGuid,
                commandInput.StandardType);
        }
        
        return ValidationResult.Success;
    }
}

public sealed class AddStandardPointsCommand : ICommand<AddStandardPointsCommandPayload, Unit>
{
    private readonly ApplicationContext _applicationContext;
    private readonly AddStandardPointsCommandValidator _validator;

    public AddStandardPointsCommand(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _validator = new AddStandardPointsCommandValidator(applicationContext);
    }

    public async Task<Result<Unit>> ExecuteAsync(AddStandardPointsCommandPayload commandPayload)
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
        
        var standardsStudentHistoryEntity = new StandardsStudentHistoryEntity
        {
            StudentGuid = commandPayload.StudentGuid,
            TeacherGuid = commandPayload.TeacherGuid,
            SemesterName = student.CurrentSemesterName,
            Date = commandPayload.Date,
            Points = commandPayload.Points,
            StandardType = commandPayload.StandardType
        };

        int adjustedStudentPointsAmount;
        if (student.PointsForStandards + standardsStudentHistoryEntity.Points > MAX_POINTS_FOR_STANDARDS)
        {
            adjustedStudentPointsAmount = MAX_POINTS_FOR_STANDARDS;
        }
        else
        {
            adjustedStudentPointsAmount = standardsStudentHistoryEntity.Points;
        }

        student.PointsForStandards = adjustedStudentPointsAmount;
        
        _applicationContext.StandardsStudentsHistory.Add(standardsStudentHistoryEntity);
        _applicationContext.Students.Update(student);
        await _applicationContext.SaveChangesAsync();

        await StudentArchiver.TryArchiveStudentIfHisDebtIsClosed(student, _applicationContext);
        
        return Unit.Default;
    }
}