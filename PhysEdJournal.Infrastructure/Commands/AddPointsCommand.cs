using LanguageExt;
using LanguageExt.Common;
using PhysEdJournal.Core.Entities.Types;
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
    private readonly ApplicationContext _applicationContext;

    public AddPointsCommandValidator(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public async ValueTask<ValidationResult> ValidateCommandInputAsync(AddPointsCommandPayload commandInput)
    {
        throw new NotImplementedException();
    }
}

public sealed class AddPointsCommand : ICommand<AddPointsCommandPayload, Unit>
{
    public async Task<Result<Unit>> ExecuteAsync(AddPointsCommandPayload commandPayload)
    {
        throw new NotImplementedException();
    }
}