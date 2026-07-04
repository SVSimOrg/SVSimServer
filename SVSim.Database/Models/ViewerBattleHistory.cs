namespace SVSim.Database.Models;

/// <summary>
/// One row per recent battle the viewer participated in, surfaced by /replay/info.
/// Composite PK on (ViewerId, BattleId). Retention: 50 rows per viewer, oldest
/// evicted on insert (see <see cref="Services.Replay.BattleHistoryWriter"/>).
///
/// The battle payload itself is NOT stored here — the client uses its local
/// <c>NewReplay/&lt;battle_id&gt;/</c> cache for playback. See
/// <c>docs/superpowers/specs/2026-06-10-replay-info-design.md</c>.
/// </summary>
public class ViewerBattleHistory
{
    public long ViewerId { get; set; }
    public long BattleId { get; set; }

    public int BattleType { get; set; }
    public int DeckFormat { get; set; }
    public int TwoPickType { get; set; }
    public int IsLimitTurn { get; set; }

    public int SelfClassId { get; set; }
    public int SelfSubClassId { get; set; }
    public int SelfCharaId { get; set; }
    public string SelfRotationId { get; set; } = "0";

    public int    OpponentClassId { get; set; }
    public int    OpponentSubClassId { get; set; }
    public int    OpponentCharaId { get; set; }
    public string OpponentName { get; set; } = "";
    public string OpponentCountryCode { get; set; } = "";
    public long   OpponentEmblemId { get; set; }
    public long   OpponentDegreeId { get; set; }
    public string OpponentRotationId { get; set; } = "0";

    public bool     IsWin { get; set; }
    public DateTime BattleStartTime { get; set; }
    public DateTime CreateTime { get; set; }
}
