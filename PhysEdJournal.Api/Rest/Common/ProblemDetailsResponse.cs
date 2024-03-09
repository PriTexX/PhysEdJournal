namespace PhysEdJournal.Api.Rest.Common;

public sealed class ProblemDetailsResponse
{
    public string? Success { get; init; }

    public required string Type { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public required string Detail { get; init; }
}
