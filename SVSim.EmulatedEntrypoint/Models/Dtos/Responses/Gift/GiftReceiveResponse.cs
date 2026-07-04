using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Gift;

[MessagePackObject]
public class GiftReceiveResponse
{
    /// <summary>Cards granted (always empty for tutorial — the starter bundle has no card-type rewards).</summary>
    [JsonPropertyName("card_list")]
    [Key("card_list")]
    public List<object> CardList { get; set; } = new();

    [JsonPropertyName("received_ids")]
    [Key("received_ids")]
    public List<string> ReceivedIds { get; set; } = new();

    [JsonPropertyName("total_receive_count_list")]
    [Key("total_receive_count_list")]
    public List<TotalReceiveCountDto> TotalReceiveCountList { get; set; } = new();

    [JsonPropertyName("present_list")]
    [Key("present_list")]
    public List<PresentDto> PresentList { get; set; } = new();

    [JsonPropertyName("present_history_list")]
    [Key("present_history_list")]
    public List<PresentDto> PresentHistoryList { get; set; } = new();

    [JsonPropertyName("is_unreceived_present")]
    [Key("is_unreceived_present")]
    public bool IsUnreceivedPresent { get; set; }

    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<GiftRewardListEntry> RewardList { get; set; } = new();

    /// <summary>
    /// Tutorial step the server is advancing the viewer to as a side-effect of this claim.
    /// Nullable: omitted via global WhenWritingNull on non-tutorial uses (none yet) or when
    /// the viewer is already past the 31→41 boundary.
    /// </summary>
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int? TutorialStep { get; set; }
}

/// <summary>
/// Per-reward summary. Prod wire shape: reward_type/reward_detail_id/reward_count are ints
/// (NOT strings, unlike PresentDto). item_type is int (0 for currency, 1/2 for items).
/// </summary>
[MessagePackObject]
public class TotalReceiveCountDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_count")]
    [Key("reward_count")]
    public long RewardCount { get; set; }

    /// <summary>0 for currency rewards, 1 or 2 for item rewards. Prod wire is int; the client's .ToInt() handles both int and string values.</summary>
    [JsonPropertyName("item_type")]
    [Key("item_type")]
    public int ItemType { get; set; }

    [JsonPropertyName("is_usable")]
    [Key("is_usable")]
    public bool IsUsable { get; set; } = true;
}

/// <summary>
/// Entries in /tutorial/gift_receive's reward_list. Wire shape: reward_type and reward_id are
/// STRINGS, reward_num is INT for currency entries (type 1, 9) and STRING for item entries
/// (type 4). Use string for reward_num to handle both — the client tolerates string→int parse.
/// </summary>
[MessagePackObject]
public class GiftRewardListEntry
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public string RewardType { get; set; } = string.Empty;

    [JsonPropertyName("reward_id")]
    [Key("reward_id")]
    public string RewardId { get; set; } = "0";

    [JsonPropertyName("reward_num")]
    [Key("reward_num")]
    public string RewardNum { get; set; } = "0";
}
