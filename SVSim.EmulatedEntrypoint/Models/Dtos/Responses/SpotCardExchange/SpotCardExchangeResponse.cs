using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.SpotCardExchange;

/// <summary>
/// /spot_card_exchange/exchange response. <c>reward_list</c> entries follow the standard
/// shape: SpotCardPoint debit post-state first, then the card grant (with cosmetic cascade
/// if applicable).
/// </summary>
[MessagePackObject]
public class SpotCardExchangeResponse
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
