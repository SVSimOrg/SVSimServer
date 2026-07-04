using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class BaseRequest
{
    [JsonPropertyName("viewer_id")]
    [Key("viewer_id")]
    public string ViewerId { get; set; }
    [JsonPropertyName("steam_id")]
    [Key("steam_id")]
    public ulong SteamId { get; set; }
    [JsonPropertyName("steam_session_ticket")]
    [Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; }
}