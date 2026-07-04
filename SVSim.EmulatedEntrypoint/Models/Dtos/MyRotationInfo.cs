using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class MyRotationInfo
{
    [JsonPropertyName("abilities")]
    [Key("abilities")]
    public Dictionary<string, MyRotationAbility> Abilities { get; set; } = new Dictionary<string, MyRotationAbility>();
    [JsonPropertyName("schedules")]
    [Key("schedules")]
    public SpecialRotationSchedule Schedules { get; set; } = new SpecialRotationSchedule();
    [JsonPropertyName("setting")]
    [Key("setting")]
    public Dictionary<string, SpecialRotationSetting>? Settings { get; set; }
    /// <summary>Prod wire key is <c>disable_card_set_ids</c> (no trailing 'd' on "disable").</summary>
    [JsonPropertyName("disable_card_set_ids")]
    [Key("disable_card_set_ids")]
    public List<int>? DisabledCardSets { get; set; }
    
    /// <summary>
    /// Set to card to card reprinted list.
    /// </summary>
    [JsonPropertyName("reprinted_base_card_ids")]
    [Key("reprinted_base_card_ids")]
    public Dictionary<string, Dictionary<string, int>>? ReprintedCards { get; set; }


    /// <summary>
    /// Set to card to count banlist.
    /// </summary>
    [JsonPropertyName("restricted_base_card_id_list")]
    [Key("restricted_base_card_id_list")]
    public Dictionary<string, Dictionary<string, int>>? Banlist { get; set; }
}