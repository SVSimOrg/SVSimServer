// AUTHORED SHIM (pass-5): ReplayDataHandler kept as minimal no-op subclass of
// WatchDataHandler. Original auto-generated ctor chained `: base(nBattleMgr, room, ...)`
// against a 5-arg WatchDataHandler ctor that referenced types (NetworkWatchBattleMgr,
// RoomConnectController) scheduled for deletion; removed the ctor since nothing
// constructs `new ReplayDataHandler(...)` anywhere. Methods are the two the
// (still-live) NewReplayBattleMgr / NetworkReplayBattleMgr call.
namespace Wizard.RoomMatch
{
}
