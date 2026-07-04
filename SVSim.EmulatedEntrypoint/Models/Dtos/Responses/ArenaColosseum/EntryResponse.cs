using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/entry</c>. Sparse — only <c>reward_list</c> (wallet debit) +
/// <c>entry_info.deck_format</c>. Client refreshes full lobby state via the next <c>/top</c>.
/// Reuses <see cref="RewardEntryDto"/> from arena-two-pick — the wire shape is identical
/// (<c>reward_type/reward_id/reward_num</c> per <c>UpdateHaveUserGoodsNumByJsonData</c>).
/// </summary>
[MessagePackObject]
public class EntryResponse
{
    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<RewardEntryDto> RewardList { get; set; } = new();

    [JsonPropertyName("entry_info")] [Key("entry_info")]
    public ColosseumEntryRef EntryInfo { get; set; } = new();
}
