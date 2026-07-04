using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.LeaderSkin;

/// <summary>
/// /leader_skin/ids — flat list of leader_skin_ids the viewer owns. Used by the client to
/// refresh badges across the skin-selection UI without re-fetching the full shop catalog.
/// </summary>
[MessagePackObject]
public class LeaderSkinIdsResponse
{
    [JsonPropertyName("user_leader_skin_ids")]
    [Key("user_leader_skin_ids")]
    public List<int> UserLeaderSkinIds { get; set; } = new();
}
