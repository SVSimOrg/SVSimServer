using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.LeaderSkin;

/// <summary>
/// POST /leader_skin/set — the per-class "current leader skin" preference used as a fallback
/// when a deck has <c>leader_skin_id == 0</c>. Two modes:
///   - Non-random: <c>is_random_leader_skin=false</c>, <c>leader_skin_id</c> is the chosen skin id.
///   - Random:     <c>is_random_leader_skin=true</c>, <c>leader_skin_id_list</c> is the shuffle pool
///     (server picks per-match). Random mode is not implemented in v1 (returns 501).
/// Source: <c>Wizard/LeaderSkinUpdateTask.cs</c>.
/// </summary>
[MessagePackObject]
public class LeaderSkinSetRequest : BaseRequest
{
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }

    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")]
    public bool IsRandomLeaderSkin { get; set; }

    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")]
    public int[] LeaderSkinIdList { get; set; } = Array.Empty<int>();
}
