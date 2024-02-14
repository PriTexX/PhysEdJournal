using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PhysEdJournal.Api.Api.ResponseType;

[JsonConverter(typeof(CustomProblemDetailsJsonConverter))]
public sealed class ProblemDetailsResponse : RestResponse
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("detail")]
    public required string Detail { get; init; }

    [JsonExtensionData]
    public Dictionary<string, object?>? Extensions { get; init; }
}

public sealed class ProblemDetailsResult : IActionResult
{
    private readonly ProblemDetailsResponse _problemDetails;

    public ProblemDetailsResult(ProblemDetailsResponse problemDetails)
    {
        _problemDetails = problemDetails;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = new ObjectResult(_problemDetails)
        {
            StatusCode = _problemDetails.Status,
        };
        return objectResult.ExecuteResultAsync(context);
    }
}

public sealed class CustomProblemDetailsJsonConverter : JsonConverter<ProblemDetailsResponse>
{
    private static readonly JsonEncodedText Success = JsonEncodedText.Encode("success");
    private static readonly JsonEncodedText Type = JsonEncodedText.Encode("type");
    private static readonly JsonEncodedText Title = JsonEncodedText.Encode("title");
    private static readonly JsonEncodedText Status = JsonEncodedText.Encode("status");
    private static readonly JsonEncodedText Detail = JsonEncodedText.Encode("detail");

    public override ProblemDetailsResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException(); // Этот конвертер только для ответа сервера, поэтому нет смысла реализовывать чтение данных
    }

    public override void Write(
        Utf8JsonWriter writer,
        ProblemDetailsResponse value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        WriteProblemDetails(writer, value, options);
        writer.WriteEndObject();
    }

    private static void WriteProblemDetails(
        Utf8JsonWriter writer,
        ProblemDetailsResponse value,
        JsonSerializerOptions options
    )
    {
        writer.WriteBoolean(Success, value.Success);

        writer.WriteString(Type, value.Type);

        writer.WriteString(Title, value.Title);

        writer.WriteNumber(Status, value.Status);

        writer.WriteString(Detail, value.Detail);

        if (value.Extensions is not null)
        {
            foreach (var kvp in value.Extensions)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(
                    writer,
                    kvp.Value,
                    kvp.Value?.GetType() ?? typeof(object),
                    options
                );
            }
        }
    }
}
