namespace SVSim.Database.Services.Replay;

/// <summary>
/// Read-side row returned by <see cref="IReplayHistoryReader"/>. The /replay/info
/// controller maps this to its wire DTO (all-stringified per prod capture).
/// </summary>
public sealed record ReplayHistoryEntry(
    long   BattleId,
    int    BattleType,
    int    DeckFormat,
    int    TwoPickType,
    int    IsLimitTurn,
    int    SelfClassId,
    int    SelfSubClassId,
    int    SelfCharaId,
    string SelfRotationId,
    int    OpponentClassId,
    int    OpponentSubClassId,
    int    OpponentCharaId,
    string OpponentName,
    string OpponentCountryCode,
    long   OpponentEmblemId,
    long   OpponentDegreeId,
    string OpponentRotationId,
    bool   IsWin,
    DateTime BattleStartTime,
    DateTime CreateTime);
