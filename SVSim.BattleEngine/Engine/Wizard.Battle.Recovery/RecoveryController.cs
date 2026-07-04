namespace Wizard.Battle.Recovery;

// PASS-5 STUB: full body dropped. RecoveryController is client-side coroutine machinery
// (Matching_*, RoomConnectController.BattleRule, RoomBase.SettingOpponentDispData etc.)
// that never runs in the headless node — nothing constructs it (RecoveryNetworkBattleMgr-
// ContentsCreator was deleted in this pass). The type must still exist as a compile-time
// reference because:
//   1. NetworkBattleManagerBase.cs:96 declares `public RecoveryController _recoveryController;`
//      as a field that is always null in the node context.
//   2. RecoveryOperationCollection.cs:42 dereferences that field via
//      `_recoveryController.RecoveryDataHandlerInstance.OnCompleteRecovery` on the
//      IsRecovery=true headless path — the resulting NRE is the pinned failure that
//      Spellboost_play_resolution_under_random_draw_plus_spin and Capture_replay_reproduces_
//      post_mulligan_divergence rely on as their expected divergence signal.
//   3. RecoveryDataHandler.cs (also stubbed this pass) touches _recoveryController.IsMariganFinished.
// The two surface members below are the entire external contract.
public class RecoveryController
{
    public RecoveryDataHandler RecoveryDataHandlerInstance => null;
}
