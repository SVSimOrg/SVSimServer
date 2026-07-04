using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

[MessagePackObject]
public class ColosseumBattleResults
{
    [JsonPropertyName("win_count")] [Key("win_count")]
    public int WinCount { get; set; }

    /// <summary>0 = loss, 1 = win. Client iterates as bool list.</summary>
    [JsonPropertyName("result_list")] [Key("result_list")]
    public List<int> ResultList { get; set; } = new();
}
