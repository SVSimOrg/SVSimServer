using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class LeaderSelectRequest : BaseRequest
{
    [JsonPropertyName("section_id")]
    [Key("section_id")]
    public int SectionId { get; set; }
}

[MessagePackObject]
public class LeaderSelectResponse
{
    [JsonPropertyName("leader_list")]
    [Key("leader_list")]
    public List<LeaderEntry> LeaderList { get; set; } = new();

    [JsonPropertyName("leader_count")]
    [Key("leader_count")]
    public int LeaderCount { get; set; } = 8;
}

[MessagePackObject]
public class LeaderEntry
{
    [JsonPropertyName("chara_id")]
    [Key("chara_id")]
    public int CharaId { get; set; }

    [JsonPropertyName("is_skipped")]
    [Key("is_skipped")]
    public bool IsSkipped { get; set; }

    [JsonPropertyName("is_finished")]
    [Key("is_finished")]
    public bool IsFinished { get; set; }

    [JsonPropertyName("current_chapter")]
    [Key("current_chapter")]
    public int CurrentChapter { get; set; }
}
