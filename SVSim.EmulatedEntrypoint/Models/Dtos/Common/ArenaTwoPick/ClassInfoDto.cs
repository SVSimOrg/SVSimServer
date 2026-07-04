using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

[MessagePackObject]
public class ClassInfoDto
{
    [JsonPropertyName("class_id_1")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_id_1")]
    public int ClassId1 { get; set; }

    [JsonPropertyName("class_id_2")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_id_2")]
    public int ClassId2 { get; set; }

    [JsonPropertyName("class_id_3")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_id_3")]
    public int ClassId3 { get; set; }

    [JsonPropertyName("selected_class_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("selected_class_id")]
    public int SelectedClassId { get; set; }
}
