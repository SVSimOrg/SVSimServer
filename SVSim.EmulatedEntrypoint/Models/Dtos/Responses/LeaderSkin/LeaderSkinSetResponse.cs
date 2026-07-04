using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.LeaderSkin;

/// <summary>
/// Response shape for POST /leader_skin/set. Per <c>LeaderSkinUpdateTask.Parse</c>:
///   - <c>is_random_leader_skin</c> echoes the mode the server actually applied.
///   - <c>leader_skin_id</c> is only consumed by the client when random mode is on (it picks
///     one of the pool to display). In non-random mode the client uses the request's id.
///   - <c>leader_skin_id_list</c> is the active shuffle pool (empty for non-random).
/// </summary>
[MessagePackObject]
public class LeaderSkinSetResponse
{
    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")]
    public bool IsRandomLeaderSkin { get; set; }

    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }

    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")]
    public List<int> LeaderSkinIdList { get; set; } = new();
}
