namespace SVSim.BattleNode.Protocol;

/// <summary>
/// The "resultCode" field on synchronize pushes. 1 = Success, else error.
/// Mirrors the full catalog from docs/api-spec/in-battle/enums.md, including
/// source typos in the original spec (RoomBattleReadeError, RoomTornament*).
/// </summary>
public enum ReceiveNodeResultCode
{
    None = 0,
    Success = 1,
    Different_UUID = 30001,
    RedisReplyError = 30002,
    UnexistUserinfoError = 30003,
    RoomStatusInfoError = 30101,
    RoomCreateError = 30102,
    RoomEntryError = 30103,
    RoomKickError = 30104,
    RoomLeaveError = 30105,
    RoomReleaseError = 30106,
    RoomForceReleaseError = 30107,
    RoomReenterError = 30108,
    RoomBattleReadeError = 30109,         // source typo per spec, preserved
    RoomTournamentDeckError = 30110,
    RoomTournamentError = 30111,
    RoomSetupLock = 30112,
    MatchingTimeOut = 30201,
    UnmatchedError = 30211,
    CurrentBattleError = 30212,
    UnexpectedPhaseError = 30213,
    WatchError = 30302,
    SwapTimeoutError = 31001,
    FoundRemovedUserErrorSelf = 32101,
    FoundRemovedUserErrorOppo = 32102,
    FoundRemovedUserErrorWatcher = 32103,
    RoomTimeEndError = 32104,
    WatcherInRemovedOwnerRoomError = 32105,
    RoomTornamentOwnTimeEndError = 32106,   // source typo per spec, preserved
    RoomTornamentOppoTimeEndError = 32107,  // source typo per spec, preserved
    BattleFinishTimeEnd = 32108,
}
