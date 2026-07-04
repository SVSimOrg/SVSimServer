using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;

[MessagePackObject]
public sealed class ArenaColosseumCardChooseRequest : BaseRequest
{
    [JsonPropertyName("selected_id")] [Key("selected_id")]
    public long SelectedId { get; set; }
}
