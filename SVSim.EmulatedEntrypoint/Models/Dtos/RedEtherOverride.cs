using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// An indication that a specific card has had it's red ether amounts overriden from the normal amounts.
/// </summary>
[MessagePackObject]
public class RedEtherOverride
{
    /// <summary>
    /// The id of the affected card.
    /// </summary>
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public ulong CardId { get; set; }
    
    /// <summary>
    /// How much red ether is now provided from dusting the card.
    /// </summary>
    [JsonPropertyName("get_red_ether")]
    [Key("get_red_ether")]
    public int GetRedEther { get; set; }
    
    /// <summary>
    /// How much red ether is now required to craft the card.
    /// </summary>
    [JsonPropertyName("use_red_ether")]
    [Key("use_red_ether")]
    public int UseRedEther { get; set; }
}