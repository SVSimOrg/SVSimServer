using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class CardChooseRequest : BaseRequest
{
    [JsonPropertyName("selected_id")] [Key("selected_id")] public long SelectedId { get; set; }
}
