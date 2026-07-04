using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.MyPage;

/// <summary>
/// Spec source: docs/api-spec/endpoints/post-login/mypage-finish-battle.md.
/// We always emit check_unfinished_battle=0 because no in-flight battle state
/// is tracked server-side yet. When that lands, branch to is_win + class/rank refresh.
/// </summary>
[MessagePackObject]
public class MyPageFinishBattleResponse
{
    [JsonPropertyName("check_unfinished_battle")]
    [Key("check_unfinished_battle")]
    public int CheckUnfinishedBattle { get; set; }
}
