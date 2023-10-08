using LanguageExt.Common;

namespace PhysEdJournal.Infrastructure.Commands.ValidationAndCommandAbstractions;

public interface ICommand<TPayload, TOutput>
    where TPayload : class
{
    public Task<Result<TOutput>> ExecuteAsync(TPayload commandPayload);
}
