using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Emblem;

[MessagePackObject]
public class EmblemUpdateRequest : BaseRequest
{
    // Spec note: emblem_id is a long on the wire (UserInfo.SelectedEmblemId is also long).
    [JsonPropertyName("emblem_id")]
    [Key("emblem_id")]
    public long EmblemId { get; set; }
}
