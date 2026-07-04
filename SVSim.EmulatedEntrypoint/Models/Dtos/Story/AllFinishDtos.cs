using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class AllFinishRequest : BaseRequest
{
    [JsonPropertyName("story_ids")]
    [Key("story_ids")]
    public int[] StoryIds { get; set; } = Array.Empty<int>();

    [JsonPropertyName("is_finish")]
    [Key("is_finish")]
    public int IsFinish { get; set; }
}
