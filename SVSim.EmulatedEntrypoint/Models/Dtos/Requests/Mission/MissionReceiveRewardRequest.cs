using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Mission;

/// <summary>
/// INFERRED shape — the spec at docs/api-spec/endpoints/post-login/mission-receive-reward.md
/// flags this as not present in the client decompilation. Almost certainly an id of the
/// mission to claim.
/// </summary>
[MessagePackObject]
public class MissionReceiveRewardRequest : BaseRequest
{
    [JsonPropertyName("id")]
    [Key("id")]
    public long Id { get; set; }
}
