using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

/// <summary>
/// One friend entry. Mirrors the prod capture's 15-field shape exactly — numeric fields
/// (viewer_id, rank, emblem_id, degree_id) ship as native ints; everything else as
/// stringified ints / strings.
/// </summary>
[MessagePackObject]
public sealed class FriendEntryDto
{
    [JsonPropertyName("device_type")][Key("device_type")] public string DeviceType { get; set; } = "0";
    [JsonPropertyName("name")][Key("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("country_code")][Key("country_code")] public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("max_friend")][Key("max_friend")] public string MaxFriend { get; set; } = "0";
    [JsonPropertyName("last_play_time")][Key("last_play_time")] public string LastPlayTime { get; set; } = string.Empty;
    [JsonPropertyName("is_received_two_pick_mission")][Key("is_received_two_pick_mission")] public string IsReceivedTwoPickMission { get; set; } = "0";
    [JsonPropertyName("birth")][Key("birth")] public string Birth { get; set; } = "0";
    [JsonPropertyName("mission_change_time")][Key("mission_change_time")] public string MissionChangeTime { get; set; } = string.Empty;
    [JsonPropertyName("mission_receive_type")][Key("mission_receive_type")] public string MissionReceiveType { get; set; } = "0";
    [JsonPropertyName("is_official")][Key("is_official")] public string IsOfficial { get; set; } = "0";
    [JsonPropertyName("is_official_mark_displayed")][Key("is_official_mark_displayed")] public string IsOfficialMarkDisplayed { get; set; } = "0";
    [JsonPropertyName("viewer_id")][Key("viewer_id")] public int ViewerId { get; set; }
    [JsonPropertyName("rank")][Key("rank")] public int Rank { get; set; }
    [JsonPropertyName("emblem_id")][Key("emblem_id")] public long EmblemId { get; set; }
    [JsonPropertyName("degree_id")][Key("degree_id")] public int DegreeId { get; set; }
}
