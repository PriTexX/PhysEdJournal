using System.Text.Json.Serialization;

namespace PhysEdJournal.Core.Entities.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkType
{
    ExternalFitness,
    GTO,
    Science,
    OnlineWork, // СДО
    InternalTeam, // Сборная
    Activist,
    Competition,
}
