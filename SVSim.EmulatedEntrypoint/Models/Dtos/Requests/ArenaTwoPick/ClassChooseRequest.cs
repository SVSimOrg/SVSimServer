using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class ClassChooseRequest : BaseRequest
{
    [JsonPropertyName("class_id")] [Key("class_id")] public int ClassId { get; set; }
}
