namespace PhysEdJournal.Api.Api.ResponseType;

public sealed class OkResponse : RestResponse
{
    public object? Data { get; init; }

    public OkResponse()
    {
        Success = true;
    }
}
