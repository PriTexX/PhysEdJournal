using System.Text.Json;

namespace Admin.Api;

public static class UnpackJsonElement
{
    public static object? Unpack(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var value))
        {
            return value;
        }

        if (json.ValueKind == JsonValueKind.Number && json.TryGetDouble(out var dValue))
        {
            return dValue;
        }

        switch (json.ValueKind)
        {
            case JsonValueKind.String:
                return json.ToString();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return json.GetBoolean();
            case JsonValueKind.Null:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
