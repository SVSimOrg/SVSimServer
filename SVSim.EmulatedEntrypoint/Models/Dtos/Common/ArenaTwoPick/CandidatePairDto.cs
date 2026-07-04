using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

[MessagePackObject]
public class CandidatePairDto
{
    [JsonPropertyName("id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("id")]
    public long Id { get; set; }

    [JsonPropertyName("turn")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("turn")]
    public int Turn { get; set; }

    [JsonPropertyName("set_num")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("set_num")]
    public int SetNum { get; set; }

    [JsonPropertyName("card_id_1")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("card_id_1")]
    public long CardId1 { get; set; }

    [JsonPropertyName("card_id_2")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("card_id_2")]
    public long CardId2 { get; set; }

    [JsonPropertyName("is_selected")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("is_selected")]
    public int IsSelected { get; set; }
}
