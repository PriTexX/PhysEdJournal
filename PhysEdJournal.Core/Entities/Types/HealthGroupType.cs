using System.Text.Json.Serialization;

namespace PhysEdJournal.Core.Entities.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthGroupType
{
    None,
    Basic,
    Preparatory,
    Special,
    HealthLimitations,
}
