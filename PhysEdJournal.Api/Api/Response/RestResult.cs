using System.Text.Json.Serialization;

namespace PhysEdJournal.Api.Api._Response;

public class RestResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }
}
