using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;

/// <summary>
/// /deck/update response. Minimum-viable per spec is just {achieved_info, reward_list};
/// the full shape also includes the refreshed deck list. We include user_deck_list to
/// save the client a follow-up /deck/info round-trip.
/// </summary>
[MessagePackObject]
public class DeckUpdateResponse
{
    [JsonPropertyName("user_deck_list")]
    [Key("user_deck_list")] public List<UserDeck>? UserDeckList { get; set; }
    [JsonPropertyName("achieved_info")]
    [Key("achieved_info")] public Dictionary<string, object> AchievedInfo { get; set; } = new();
    [JsonPropertyName("reward_list")]
    [Key("reward_list")] public List<Reward> RewardList { get; set; } = new();
}
