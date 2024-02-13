namespace PhysEdJournal.Api.Api;

public sealed class OkResult : RestResult
{
    public object? Data { get; init; }
}
