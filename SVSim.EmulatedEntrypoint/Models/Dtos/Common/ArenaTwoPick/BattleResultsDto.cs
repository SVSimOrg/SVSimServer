using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

[MessagePackObject]
public class BattleResultsDto
{
    /// <summary>Each entry is 0 (loss) or 1 (win). Native int array — matches capture.</summary>
    [JsonPropertyName("result_list")] [Key("result_list")]
    public List<int> ResultList { get; set; } = new();

    [JsonPropertyName("win_count")] [Key("win_count")]
    public int WinCount { get; set; }
}
