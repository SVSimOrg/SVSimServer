using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/change_role. Change a member's role (promote/demote/transfer leadership).</summary>
[MessagePackObject]
public class GuildChangeRoleRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>ViewerId of the member whose role is changing.</summary>
    [JsonPropertyName("target_viewer_id"), Key("target_viewer_id")]
    public long TargetViewerId { get; set; }

    /// <summary>New role: 0 = REGULAR, 1 = LEADER, 2 = SUB_LEADER.</summary>
    [JsonPropertyName("role_id"), Key("role_id")]
    public int RoleId { get; set; }
}
