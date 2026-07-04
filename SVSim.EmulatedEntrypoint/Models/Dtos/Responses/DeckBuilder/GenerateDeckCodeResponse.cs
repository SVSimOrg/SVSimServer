using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;

[MessagePackObject]
public class GenerateDeckCodeResponse
{
    [JsonPropertyName("text")]
    [Key("text")]
    public string Text { get; set; } = "OK";

    [JsonPropertyName("deck_code")]
    [Key("deck_code")]
    public string DeckCode { get; set; } = "";

    [JsonPropertyName("errors")]
    [Key("errors")]
    public PortalErrors Errors { get; set; } = new();
}
