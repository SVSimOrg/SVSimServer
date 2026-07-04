using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/event_info</c>. The 3-round Colosseum bracket descriptor — note
/// the rounds are STRING-keyed (<c>"1"</c>, <c>"2"</c>, <c>"3"</c>), NOT an array. The
/// client iterates <c>for (i = 1; i &lt;= 3; i++) jsonData[i.ToString()]</c>. Using three
/// explicit <c>[JsonPropertyName("1"|"2"|"3")]</c> properties is simpler than a custom STJ
/// converter and round-trips cleanly through MessagePack via matching <c>[Key("1"|...)]</c>.
/// </summary>
[MessagePackObject]
public class EventInfoResponse
{
    [JsonPropertyName("colosseum_info")] [Key("colosseum_info")]
    public ColosseumEventInfo ColosseumInfo { get; set; } = new();

    [JsonPropertyName("1")] [Key("1")]
    public ColosseumRoundDetail Round1 { get; set; } = new();

    [JsonPropertyName("2")] [Key("2")]
    public ColosseumRoundDetail Round2 { get; set; } = new();

    [JsonPropertyName("3")] [Key("3")]
    public ColosseumRoundDetail Round3 { get; set; } = new();

    [JsonPropertyName("colosseum_status")] [Key("colosseum_status")]
    public ColosseumOwnStatus ColosseumStatus { get; set; } = new();
}
