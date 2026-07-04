using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

[MessagePackObject]
public class ColosseumFeeList
{
    [JsonPropertyName("rupy_cost")] [Key("rupy_cost")]
    public int RupyCost { get; set; }

    [JsonPropertyName("ticket_cost")] [Key("ticket_cost")]
    public int TicketCost { get; set; }

    [JsonPropertyName("crystal_cost")] [Key("crystal_cost")]
    public int CrystalCost { get; set; }
}
