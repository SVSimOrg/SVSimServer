using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/cancel_invite. Leader/sub-leader retracts an outstanding invite.</summary>
[MessagePackObject]
public class GuildCancelInviteRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Invite id from /guild/invite_user_list.</summary>
    [JsonPropertyName("invite_id"), Key("invite_id")]
    public long InviteId { get; set; }
}
