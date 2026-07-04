using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Emblem;

[MessagePackObject]
public class EmblemListResponse
{
    [JsonPropertyName("user_emblem_list")]
    [Key("user_emblem_list")]
    public List<EmblemListEntry> UserEmblemList { get; set; } = new();
}
