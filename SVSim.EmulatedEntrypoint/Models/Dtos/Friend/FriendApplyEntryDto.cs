using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class FriendApplyEntryDto
{
    [JsonPropertyName("id")][Key("id")] public int Id { get; set; }
    [JsonPropertyName("viewer_id")][Key("viewer_id")] public int ViewerId { get; set; }
    [JsonPropertyName("name")][Key("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("country_code")][Key("country_code")] public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("rank")][Key("rank")] public int Rank { get; set; }
    [JsonPropertyName("emblem_id")][Key("emblem_id")] public long EmblemId { get; set; }
    [JsonPropertyName("degree_id")][Key("degree_id")] public int DegreeId { get; set; }
    [JsonPropertyName("last_play_time")][Key("last_play_time")] public string LastPlayTime { get; set; } = string.Empty;
    [JsonPropertyName("create_time")][Key("create_time")] public string CreateTime { get; set; } = string.Empty;

    /// <summary>Only emitted when non-zero (matches prod's optional shape).</summary>
    [JsonPropertyName("mission_type")][Key("mission_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MissionType { get; set; }
}
