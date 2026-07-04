using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Wire;

/// <summary>Shared System.Text.Json options for the bare-camelCase Socket.IO / Engine.IO wire:
/// per-field <c>[JsonPropertyName]</c> casing (NOT EmulatedEntrypoint's snake_case policy), null
/// fields omitted, and unattributed enums written as their name. Single-sourced here because
/// <see cref="EngineIoHandshake"/> and <see cref="Protocol.MsgEnvelope"/> previously each built a
/// byte-identical block in their own namespace — a drift hazard.</summary>
internal static class WireJsonOptions
{
    public static readonly JsonSerializerOptions CamelCase = Create();

    private static JsonSerializerOptions Create()
    {
        var opt = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        opt.Converters.Add(new JsonStringEnumConverter());
        return opt;
    }
}
