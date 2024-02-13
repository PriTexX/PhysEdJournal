namespace PhysEdJournal.Api.Api._Response;

public sealed class OkResult : RestResult
{
    public object? Data { get; init; }

    public OkResult()
    {
        Success = true;
    }
}
