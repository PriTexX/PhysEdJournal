using System.Text.Json.Serialization;

namespace PhysEdJournal.Core.Entities.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StandardType
{
    Tilts, // Наклоны
    Jumps, // Прыжки
    PullUps, // Подтягивания
    Squats, // Приседания
    JumpingRopeJumps, // Прыжки через скакалку
    TorsoLifts, // Поднимания туловища
    FlexionAndExtensionOfArms, // Сгибания и разгибания рук
    ShuttleRun, // Челночный бег
    Other,
}
