using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Per-class entry of <c>/deck/info data.user_leader_skin_setting_list</c>. Per-viewer state:
/// each viewer's class-level "active leader skin" preference, used as a fallback when a deck
/// has <c>leader_skin_id == 0</c>. Sourced from <c>ViewerClassData.LeaderSkin</c>; mutated by
/// <c>POST /leader_skin/set</c>.
/// </summary>
[MessagePackObject]
public class UserLeaderSkinSetting
{
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")]
    public int IsRandomLeaderSkin { get; set; }

    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }
}
