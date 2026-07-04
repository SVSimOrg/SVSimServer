using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/change_role.
/// Returns the full updated member list so the client can redraw without re-fetching /guild/info.
/// </summary>
[MessagePackObject]
public class GuildChangeRoleResponse
{
    [JsonPropertyName("members"), Key("members")]
    public List<GuildMemberInfoDto> Members { get; set; } = new();
}
