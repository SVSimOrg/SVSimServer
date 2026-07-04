using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/invite. Leader/sub-leader sends invite to a user.</summary>
[MessagePackObject]
public class GuildInviteRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>ViewerId of the user to invite.</summary>
    [JsonPropertyName("invited_viewer_id"), Key("invited_viewer_id")]
    public long InvitedViewerId { get; set; }

    /// <summary>Free-text invite message. Client hardcodes "" — always empty on wire.</summary>
    [JsonPropertyName("invite_message"), Key("invite_message")]
    public string InviteMessage { get; set; } = "";
}
