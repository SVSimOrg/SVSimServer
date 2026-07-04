using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// What features are restricted by a user's country's loot box regulations. 1 indicates restricted, 0 indicates 
/// </summary>
[MessagePackObject]
public class LootBoxRegulations
{
    [JsonPropertyName("pack")]
    [Key("pack")]
    public int Pack { get; set; }
    [JsonPropertyName("arena_2pick")]
    [Key("arena_2pick")]
    public int ArenaPickTwo { get; set; }
    [JsonPropertyName("arena_sealed")]
    [Key("arena_sealed")]
    public int ArenaSealed { get; set; }
    [JsonPropertyName("arena_colosseum")]
    [Key("arena_colosseum")]
    public int ArenaColosseum { get; set; }
    [JsonPropertyName("arena_competition")]
    [Key("arena_competition")]
    public int ArenaCompetition { get; set; }
    [JsonPropertyName("special_shop")]
    [Key("special_shop")]
    public int SpecialShop { get; set; }
}