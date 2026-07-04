namespace SVSim.Database.Services.Replay;

/// <summary>
/// Per-viewer battle context captured at start time (do_matching/start) and consumed
/// at finish time. Lives in <see cref="IBattleContextStore"/> for the duration of a
/// single battle. See docs/superpowers/specs/2026-06-10-replay-info-design.md.
/// </summary>
public sealed record BattleContext(
    long   BattleId,
    int    BattleType,
    int    DeckFormat,
    int    TwoPickType,
    int    SelfClassId,
    int    SelfSubClassId,
    int    SelfCharaId,
    string SelfRotationId,
    int    OpponentViewerId,
    string OpponentName,
    int    OpponentClassId,
    int    OpponentSubClassId,
    int    OpponentCharaId,
    string OpponentCountryCode,
    long   OpponentEmblemId,
    long   OpponentDegreeId,
    string OpponentRotationId,
    DateTime BattleStartTime);
