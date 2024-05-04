namespace PhysEdJournal.Api.Rest.Common;

public sealed class ErrorResponse
{
    public required int StatusCode { get; init; }

    public required string Type { get; init; }

    public required string Detail { get; init; }
}
