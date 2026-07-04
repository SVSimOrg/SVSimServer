using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.FreeBattle;

/// <summary>
/// Free-battle /finish wire response. Strict subset of the rank-battle variant —
/// FreeMatchFinishDetail : MatchFinishBase is an empty subclass (no extra fields),
/// so the rank-only block (rank / after_battle_point / after_master_point /
/// battle_point / master_point / successive_win_* / promotion / grand_master) is
/// dropped. See Shadowverse_Code_2026-05-23/FreeMatchFinishDetail.cs and
/// MatchFinishBase.cs.
///
/// Phase-3 stub: echo battle_result, emit zeros for class XP (class_level=1 so the
/// UI doesn't underflow). Real reward / class-XP / mission math is a separate spec.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public sealed class FreeBattleFinishResponseDto
{
    [JsonPropertyName("battle_result")]
    [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("get_class_experience")]
    [Key("get_class_experience")]
    public int GetClassExperience { get; set; }

    [JsonPropertyName("class_experience")]
    [Key("class_experience")]
    public int ClassExperience { get; set; }

    [JsonPropertyName("class_level")]
    [Key("class_level")]
    public int ClassLevel { get; set; } = 1;
}
