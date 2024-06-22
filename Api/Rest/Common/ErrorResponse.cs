namespace Api.Rest.Common;

public readonly struct ErrorResponse
{
    public required int StatusCode { get; init; }

    public required string Type { get; init; }

    public required string Detail { get; init; }
}
