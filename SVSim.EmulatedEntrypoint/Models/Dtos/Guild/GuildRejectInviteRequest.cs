using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/reject_invite. User declines an incoming invite.</summary>
[MessagePackObject]
public class GuildRejectInviteRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Invite id from /guild/invited_guild_list.</summary>
    [JsonPropertyName("invite_id"), Key("invite_id")]
    public long InviteId { get; set; }
}
