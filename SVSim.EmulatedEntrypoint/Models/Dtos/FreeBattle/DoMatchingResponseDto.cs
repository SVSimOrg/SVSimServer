using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.FreeBattle;

/// <summary>
/// Free-battle /do_matching wire response. Identical shape to the rank-battle variant
/// (see Models/Dtos/RankBattle/DoMatchingResponseDto.cs). The client task class
/// <c>Wizard/FreeBattleDoMatchingTask.cs</c> shares <c>DoMatchingBase.SettingDoMatchingData()</c>
/// with rank-battle, so the readable wire surface is identical.
///
/// Prod response carries additional <c>room_param</c> / <c>room_id</c> fields
/// (per traffic_prod_ranked_unlimited.ndjson) that the client does NOT read on this
/// task path — omitted in v1.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public sealed class DoMatchingResponseDto
{
    [JsonPropertyName("matching_state")]
    [Key("matching_state")]
    public int MatchingState { get; set; }

    [JsonPropertyName("timeout_period")]
    [Key("timeout_period")]
    public int TimeoutPeriod { get; set; } = 60;

    [JsonPropertyName("retry_period")]
    [Key("retry_period")]
    public int RetryPeriod { get; set; } = 3;

    [JsonPropertyName("battle_id")]
    [Key("battle_id")]
    public string? BattleId { get; set; }

    // Always emitted, even on RETRY. The client's DoMatchingBase.SettingDoMatchingData()
    // calls .ToString() on this without a Keys.Contains guard, so absence throws
    // KeyNotFoundException before the matching_state switch runs.
    [JsonPropertyName("node_server_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("node_server_url")]
    public string NodeServerUrl { get; set; } = "";

    [JsonPropertyName("card_master_id")]
    [Key("card_master_id")]
    public int? CardMasterId { get; set; }
}
