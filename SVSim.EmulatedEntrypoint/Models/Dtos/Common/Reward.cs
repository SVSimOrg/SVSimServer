using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Stub for the Reward shape (spec: common/types.ts.md#reward). Fleshed out when actual
/// reward-granting flows land. Today's endpoints all emit empty reward_list arrays.
/// </summary>
[MessagePackObject]
public class Reward
{
    [JsonPropertyName("type")]
    [Key("type")] public int? Type { get; set; }
    [JsonPropertyName("value")]
    [Key("value")] public long? Value { get; set; }
    [JsonPropertyName("num")]
    [Key("num")] public int? Num { get; set; }
}
