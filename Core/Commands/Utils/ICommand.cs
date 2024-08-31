using PResult;

namespace Core.Commands;

public interface ICommand<in TPayload, TOutput>
    where TPayload : class
{
    public Task<Result<TOutput>> ExecuteAsync(TPayload payload);
}
