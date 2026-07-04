using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.RankBattle;

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

    // Always emitted, even on RETRY. Client's DoMatchingBase.SettingDoMatchingData()
    // calls .ToString() on this without a Keys.Contains guard, so absence throws
    // KeyNotFoundException before the matching_state switch runs. Same Phase 2 fix
    // pattern as TK2.
    [JsonPropertyName("node_server_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("node_server_url")]
    public string NodeServerUrl { get; set; } = "";

    [JsonPropertyName("card_master_id")]
    [Key("card_master_id")]
    public int? CardMasterId { get; set; }
}
