using System.Text.Json.Serialization;

namespace SRF.Knx.Config.OpenHab.Templating.Items;

[Serializable]
[JsonConverter(typeof(JsonStringEnumConverter<ItemType>))]
public enum ItemType {
    Undefined,

    //
    Color,
    Contact,
    DateTime,
    Dimmer,
    Group,
    Image,
    Location,
    Number,
    Player,
    Rollershutter,
    String,
    Switch,

    //
    Invalid,
}
