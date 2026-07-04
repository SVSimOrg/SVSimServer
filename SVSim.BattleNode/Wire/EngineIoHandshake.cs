using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Wire;

/// <summary>
/// Payload of an EIO3 Open packet. Sent by the server to the client immediately after the WS upgrade.
/// </summary>
public sealed record EngineIoHandshake(
    [property: JsonPropertyName("sid")] string Sid,
    [property: JsonPropertyName("upgrades")] string[] Upgrades,
    [property: JsonPropertyName("pingInterval")] int PingInterval,
    [property: JsonPropertyName("pingTimeout")] int PingTimeout)
{
    public string ToJson() => JsonSerializer.Serialize(this, WireJsonOptions.CamelCase);
}
