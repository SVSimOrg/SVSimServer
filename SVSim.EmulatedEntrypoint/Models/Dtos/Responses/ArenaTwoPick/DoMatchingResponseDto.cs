using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

/// <remarks>
/// Wire-shape notes vs prod do_matching captures (2026-05-31 TK2 capture):
/// <list type="bullet">
///   <item>Prod sends <c>room_id</c> from matching_state 3003 onward. We deliberately
///         omit it. The client's <c>DoMatchingDetail</c> data model has no <c>room_id</c>
///         field and <c>DoMatchingBase.SettingDoMatchingData()</c> never reads the key;
///         private-room flows (<c>RoomConnectController</c>) get their room id from a
///         separate API (<c>OpenRoomBattleCreate*</c>) and don't consult this response.
///         Re-add only if a downstream consumer surfaces.</item>
///   <item><c>node_server_url</c> must always be present (empty string while waiting,
///         actual URL on SUCCEEDED/SUCCEEDED_OWNER). The client's accessor is unguarded.</item>
///   <item><c>battle_id</c> stays absent on RETRY (its accessor IS guarded via
///         <c>Keys.Contains</c>).</item>
/// </list>
/// </remarks>
[MessagePackObject]
public sealed class DoMatchingResponseDto
{
    [JsonPropertyName("matching_state")] [Key("matching_state")]
    public int MatchingState { get; set; } = 3004;  // SUCCEEDED

    [JsonPropertyName("timeout_period")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("timeout_period")]
    public int TimeoutPeriod { get; set; } = 30;

    [JsonPropertyName("retry_period")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("retry_period")]
    public int RetryPeriod { get; set; } = 3;

    [JsonPropertyName("battle_id")] [Key("battle_id")]
    public string? BattleId { get; set; }

    [JsonPropertyName("node_server_url")] [Key("node_server_url")]
    public string? NodeServerUrl { get; set; }

    // Required by the client when matching_state ∈ {3004, 3007, 3011} —
    // DoMatchingBase.SettingCardMasterId does jsonData["card_master_id"].ToInt()
    // with no Keys.Contains guard, so omitting it throws KeyNotFoundException.
    // Value matches what /load/index returns (the "current battle card master").
    [JsonPropertyName("card_master_id")] [Key("card_master_id")]
    public int CardMasterId { get; set; } = 1;

    [JsonPropertyName("room_param")] [Key("room_param")]
    public string RoomParam { get; set; } = "";

    [JsonPropertyName("mission_parameter")] [Key("mission_parameter")]
    public Dictionary<string, string> MissionParameter { get; set; } = new()
    {
        ["follower_play_count_for_mission"] = "{me.game_play_cards_other_self.unit.count}",
    };
}
