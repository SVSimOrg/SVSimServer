using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class PlayedTogetherEntryDto
{
    [JsonPropertyName("viewer_id")][Key("viewer_id")] public int ViewerId { get; set; }
    [JsonPropertyName("name")][Key("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("country_code")][Key("country_code")] public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("rank")][Key("rank")] public int Rank { get; set; }
    [JsonPropertyName("emblem_id")][Key("emblem_id")] public long EmblemId { get; set; }
    [JsonPropertyName("degree_id")][Key("degree_id")] public int DegreeId { get; set; }
    [JsonPropertyName("last_play_time")][Key("last_play_time")] public string LastPlayTime { get; set; } = string.Empty;
    [JsonPropertyName("played_time")][Key("played_time")] public string PlayedTime { get; set; } = string.Empty;
    [JsonPropertyName("friend_status")][Key("friend_status")] public int FriendStatus { get; set; }
    [JsonPropertyName("friend_apply_id")][Key("friend_apply_id")] public int FriendApplyId { get; set; }
    [JsonPropertyName("played_mode")][Key("played_mode")] public int PlayedMode { get; set; }
    [JsonPropertyName("battle_type")][Key("battle_type")] public int BattleType { get; set; }
    [JsonPropertyName("deck_format")][Key("deck_format")] public int DeckFormat { get; set; }
    [JsonPropertyName("two_pick_type")][Key("two_pick_type")] public int TwoPickType { get; set; }
}
