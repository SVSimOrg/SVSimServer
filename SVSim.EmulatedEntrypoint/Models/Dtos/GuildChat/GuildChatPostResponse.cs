using MessagePack;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>
/// Response for POST /guild_chat/post.
/// achieved_info and reward_list are optional; only present when posting triggered a mission completion.
/// TODO(task-16): populate achieved_info + reward_list shapes when chat missions are implemented.
/// </summary>
[MessagePackObject]
public class GuildChatPostResponse
{
    /// <summary>Optional. Present when posting triggered a mission completion.</summary>
    [JsonPropertyName("achieved_info"), Key("achieved_info")]
    public JsonElement? AchievedInfo { get; set; }

    /// <summary>Optional. Goods deltas from any rewards in achieved_info.</summary>
    [JsonPropertyName("reward_list"), Key("reward_list")]
    public JsonElement? RewardList { get; set; }
}
