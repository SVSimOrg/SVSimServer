using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BasicPuzzle;

[MessagePackObject]
public class FinishRequest : BaseRequest
{
    [JsonPropertyName("puzzle_id")]
    [Key("puzzle_id")]
    public int PuzzleId { get; set; }

    [JsonPropertyName("retry_count")]
    [Key("retry_count")]
    public int RetryCount { get; set; }

    [JsonPropertyName("is_win")]
    [Key("is_win")]
    public bool IsWin { get; set; }
}
