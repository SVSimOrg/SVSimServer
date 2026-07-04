using System.Collections.Concurrent;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Records which home_dialog_list entries have already been emitted to which viewer
/// during the current server-process lifetime. Used by MyPageController to suppress
/// re-firing the popup on subsequent /mypage/index calls.
///
/// Keyed by ShortUdid (stable for the viewer's lifetime), NOT the rotating SID.
/// Lifetime is the host process — restart re-fires once per viewer (documented trade
/// in docs/superpowers/specs/2026-06-08-home-dialog-list-design.md §1).
/// </summary>
public interface IHomeDialogSessionTracker
{
    /// <summary>True iff this dialog has not yet been emitted for this viewer in this
    /// process. Marks as fired on success.</summary>
    bool TryReserve(long viewerShortUdid, int dialogId);
}

public sealed class HomeDialogSessionTracker : IHomeDialogSessionTracker
{
    private readonly ConcurrentDictionary<long, HashSet<int>> _firedByViewer = new();

    public bool TryReserve(long viewerShortUdid, int dialogId)
    {
        var set = _firedByViewer.GetOrAdd(viewerShortUdid, _ => new HashSet<int>());
        // HashSet<int> is NOT thread-safe — lock on the per-viewer set instance so
        // we don't serialize across viewers.
        lock (set)
        {
            return set.Add(dialogId);
        }
    }
}
