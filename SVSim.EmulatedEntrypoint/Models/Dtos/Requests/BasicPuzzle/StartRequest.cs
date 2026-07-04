using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BasicPuzzle;

[MessagePackObject]
public class StartRequest : BaseRequest
{
    [JsonPropertyName("puzzle_id")]
    [Key("puzzle_id")]
    public int PuzzleId { get; set; }
}
