using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

// PASS-6 STUB: 578-line body dropped. NetworkWatchBattleMgr is the client-side
// spectator battle manager (watch-touch-control, watcher-leave/disconnect checkers,
// select-object-hand-card VFX). Never constructed in headless — the node uses
// HeadlessNetworkBattleMgr : NetworkStandardBattleMgr, a sibling subtree.
// NetworkWatchBattleReceiver (which IS on the headless path via
// NetworkReplayBattleReceiver's inheritance) does NOT cast its mgr ref to
// NetworkWatchBattleMgr — it works through NetworkBattleManagerBase, so the mgr
// body's absence is invisible to the receive path.
//
// The three surface members below are compile-load-bearing:
//   - disconnectDialog: field read by WatcherLeaveChecker (only constructed from
//     within this class — dies in the analyzer cascade).
//   - CreateVfxResetPositionByCardBase: called from WatchPlayCardAction via
//     `(_battleMgr as NetworkWatchBattleMgr).…` — cast returns null in headless.
//   - ToggleSelectHandCardMove: called from WatchOperationCollection.
//     SelectObjectOperation on `_watchBattleMgr`. The recovery path takes the
//     NetworkBattleManagerBase ctor overload that leaves `_watchBattleMgr` null,
//     so this method's only would-be invocation NREs; safe as a signature stub.
public class NetworkWatchBattleMgr : NetworkBattleManagerBase
{

    public NetworkWatchBattleMgr(IBattleMgrContentsCreator contentsCreator) : base(contentsCreator) { }
    public void ToggleSelectHandCardMove(BattleCardBase selectedCard, bool isOwner) { }
}
