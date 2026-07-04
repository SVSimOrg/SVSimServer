using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BasicPuzzle;

[MessagePackObject]
public class OpenPuzzleDialogRequest : BaseRequest
{
    [JsonPropertyName("puzzle_master_id")]
    [Key("puzzle_master_id")]
    public int PuzzleMasterId { get; set; }
}
