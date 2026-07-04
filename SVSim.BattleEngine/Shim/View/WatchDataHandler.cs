// AUTHORED SHIM (pass-5): WatchDataHandler is a compile-time load-bearing type — it appears
// as `WatchDataHandler handler = null` on ~6 method signatures in NetworkBattleReceiver and
// (until this pass ships) on NetworkWatchBattleReceiver. Headless callers always pass null;
// isOwner()/IsIncludedUri() are only reached from the isWatch=true (spectator) branch, which
// the node never enters. Kept as an empty class with the two members compile paths reference,
// returning defaults. Moved out of Shim/Generated/ since the auto-generated ctor referenced
// NetworkWatchBattleMgr + RoomConnectController, both scheduled for deletion in this pass.
namespace Wizard.RoomMatch;

public partial class WatchDataHandler
{
    public bool IsIncludedUri(string uri) => false;
    public bool isOwner(string idStr) => false;
}
