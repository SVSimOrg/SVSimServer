using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class SpecialRotationSetting
{
    [JsonPropertyName("rotation_id")]
    [Key("rotation_id")]
    public int RotationId { get; set; }
    
    /// <summary>
    /// Formatted as 'setid|setid|setid...'.
    /// </summary>
    [JsonPropertyName("card_set_ids")]
    [Key("card_set_ids")]
    public string CardSetIds { get; set; }
    
    /// <summary>
    /// Formatted as 'abilityid|abilityid|abilityid...'.
    /// </summary>
    [JsonPropertyName("abilities")]
    [Key("abilities")]
    public string Abilities { get; set; }
}