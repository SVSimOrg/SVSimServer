using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Tournament-window block returned by /mypage/index. Client constructs
/// ArenaCompetition(base.ResponseData) at MyPageTask.cs:110, which then reads
/// `responseData["data"]["competition_info"]["is_competition_period"]`
/// unconditionally (ArenaCompetition.cs:232-233). The remaining fields
/// (deck_format, entry_start_time, freebie_status, featured_entry_reward_list,
/// etc.) are only read when IsCompetitionPeriod is true, so the minimum-viable
/// payload while we have no tournament implementation is just the bool=false.
/// Prod emits the same `{"is_competition_period":false}` shape when no
/// tournament is active.
/// </summary>
[MessagePackObject]
public class CompetitionInfo
{
    [JsonPropertyName("is_competition_period")]
    [Key("is_competition_period")]
    public bool IsCompetitionPeriod { get; set; }
}
