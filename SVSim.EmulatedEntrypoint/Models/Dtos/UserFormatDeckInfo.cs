using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserFormatDeckInfo
{
    [JsonPropertyName("format")]
    [Key("format")] 
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("user_deck_list")]
    [Key("user_deck_list")]
    public List<UserDeck> UserDecks { get; set; } = new List<UserDeck>();
}