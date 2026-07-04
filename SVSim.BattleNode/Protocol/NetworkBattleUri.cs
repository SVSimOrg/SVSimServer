namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Discriminator for every msg/synchronize envelope. Wire form is the bare member name
/// (case-sensitive). See docs/api-spec/in-battle/enums.md.
/// </summary>
public enum NetworkBattleUri
{
    None,
    Resume,
    Retry,
    InitNetwork,
    InitBattle,
    InitRoomBattle,
    Matched,
    Loaded,
    Deal,
    Swap,
    Ready,
    TurnStart,
    TurnEndActions,
    TurnEnd,
    TurnEndFinal,
    PlayActions,
    BattleStart,
    BattleFinish,
    ChatStamp,
    Gungnir,
    Echo,
    Retire,
    OppoDisconnect,
    End,
    Judge,
    Touch,
    SelectSkill,
    SelectObject,
    SlideObject,
    TurnEndReady,
    RecoveryStart,
    RecoveryEnd,
    JudgeResult,
    Maintenance,
    ReplayFinish,
    Kill,
    Watch,
}
