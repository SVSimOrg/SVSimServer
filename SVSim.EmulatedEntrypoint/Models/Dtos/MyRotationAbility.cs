using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject()]
public class MyRotationAbility
{
    [JsonPropertyName("ability_id")]
    [Key("ability_id")]
    public int AbilityId { get; set; }
    [JsonPropertyName("add_start_pp")]
    [Key("add_start_pp")]
    public int AddStartPp { get; set; }
    [JsonPropertyName("add_start_life")]
    [Key("add_start_life")]
    public int AddStartLife { get; set; }
    [JsonPropertyName("increase_add_pptotal_amount")]
    [Key("increase_add_pptotal_amount")]
    public int IncreaseAddPpTotalAmount { get; set; }
    [JsonPropertyName("increase_add_pptotal_turn")]
    [Key("increase_add_pptotal_turn")]
    public int IncreaseAddPpTotalTurn { get; set; }
    [JsonPropertyName("ability")]
    [Key("ability")]
    public string Ability { get; set; } = string.Empty;
    [JsonPropertyName("ability_desc")]
    [Key("ability_desc")]
    public string AbilityDesc { get; set; } = string.Empty;
}