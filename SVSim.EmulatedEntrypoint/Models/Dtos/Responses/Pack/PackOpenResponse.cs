using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Pack;

[MessagePackObject]
public class PackOpenResponse
{
    [JsonPropertyName("pack_list")]
    [Key("pack_list")]
    public List<CardPackEntryDto> PackList { get; set; } = new();

    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();

    [JsonPropertyName("rewards")]
    [Key("rewards")]
    public List<object> Rewards { get; set; } = new();

    [JsonPropertyName("is_special_effect")]
    [Key("is_special_effect")]
    public bool IsSpecialEffect { get; set; }

    /// <summary>Empty array literal — matches prod when no missions completed.</summary>
    [JsonPropertyName("mission_result")]
    [Key("mission_result")]
    public List<object> MissionResult { get; set; } = new();

    /// <summary>
    /// Set only on the /tutorial/pack_open path to signal the END (100) transition inline with
    /// the pack reward. Global WhenWritingNull keeps it off the wire on regular /pack/open.
    /// </summary>
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int? TutorialStep { get; set; }
}

[MessagePackObject]
public class CardPackEntryDto
{
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }

    [JsonPropertyName("rarity")]
    [Key("rarity")]
    public int Rarity { get; set; }

    /// <summary>Always 1 per drawn slot — matches prod sample shape.</summary>
    [JsonPropertyName("number")]
    [Key("number")]
    public int Number { get; set; } = 1;
}
