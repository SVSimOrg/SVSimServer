using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class CardSetIdentifier
{
    [JsonPropertyName("card_set_id")]
    [Key("card_set_id")]
    public int SetId { get; set; }
}