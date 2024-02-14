using System.Text.Json.Serialization;

namespace PhysEdJournal.Api.Api.ResponseType;

public class RestResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }
}
